#pragma once

#include "stdafx.h"

#define TVTEST_PLUGIN_CLASS_IMPLEMENT	// プラグインをクラスとして実装
#include "TVTestPlugin.h"

using namespace TVTest;

typedef WCHAR wchar;
typedef unsigned int uint;
typedef BYTE byte;
typedef long long int64;

const int null = 0;

wchar* PluginVer = L"Tvmaid MAYA Plugin リリース 3";

//例外
class Exception
{
public:
	enum ErrorCode
	{
		NoError = 0,
		CreateShared,
		CreateWindow,
		CreateMutex,
		StartRec,
		StopRec,
		SetService,
		GetEvents,
		GetState,
		GetEnv,
		GetEventTime,
		GetTsStatus,
		OutOfShared
	};

	ErrorCode Code;
	wchar* Message;	//動的メモリを割り当てないこと

	Exception(ErrorCode code, wchar* msg)
	{
		this->Code = code;
		this->Message = msg;
	}
};

//共有メモリ
class SharedMemory
{
	HANDLE map;			//メモリマップハンドル

protected:
	byte* view;		//ポインタ

public:
	SharedMemory(wchar* name, int size)
	{
		map = CreateFileMapping((HANDLE)INVALID_HANDLE_VALUE, null, PAGE_READWRITE, 0, size, name);
		if (map == null)
			throw Exception(Exception::CreateShared, L"共有メモリの作成に失敗しました。");

		view = (byte*)MapViewOfFile(map, FILE_MAP_ALL_ACCESS, 0, 0, 0);
		if (view == null)
			throw Exception(Exception::CreateShared, L"共有メモリの作成に失敗しました。");
	}

	virtual ~SharedMemory()
	{
		UnmapViewOfFile(view);
		CloseHandle(map);
	}
};

class SharedText : public SharedMemory
{
	wchar* position;	//書き込み位置
	int length;			//バッファの長さ(文字数単位)

public:
	SharedText(wchar* name, int length) : SharedMemory(name, length * sizeof(wchar*))
	{
		this->length = length;

		Init();
	}

	void Init()
	{
		position = (wchar*)view;
		view[0] = 0;
	}

	wchar* Read()
	{
		return (wchar*)view;
	}

	void Write(wchar* format, ...)
	{
		va_list args;
		va_start(args, format);

		//length, position, viewは、すべて文字(wchar)換算
		int len = _vsnwprintf_s(position, length - (position - (wchar*)view), _TRUNCATE, format, args);
		
		if (len == -1)
		{
			va_end(args);
			throw Exception(Exception::OutOfShared, L"共有メモリが足りません。書き込めませんでした。");
		}
		va_end(args);
		position += len;
	}
};

//ライブストリーム
//0-8  : 書き込み位置(188バイト単位)
//8-12 : ストリームの長さ(188バイト単位)
//12-16: 未使用
//16-  : TSパケットバッファ
class LiveStream : public SharedMemory
{
public:
	LiveStream(wchar* name, int length) : SharedMemory(name, 16 + length * 188)
	{
		((int64*)view)[0] = 0;
		((int*)view)[2] = length;
	}

	void Write(byte* packet)
	{
		int64 wp = ((int64*)view)[0];
		int length = ((int*)view)[2];

		byte* buf = view + 16 + wp % length * 188;
		memcpy(buf, packet, 188);
		((int64*)view)[0] = wp + 1;
	}
};

//ミューテックス
class Mutex
{
	HANDLE mutex;

public:
	Mutex(wchar* name)
	{
		mutex = CreateMutex(null, FALSE, name);
		if (mutex == null)
			throw Exception(Exception::CreateMutex, L"Mutexの作成に失敗しました。");
	}

	bool GetOwner(int timeout)
	{
		DWORD err = WaitForSingleObject(mutex, timeout);
		return (err == WAIT_ABANDONED || err == WAIT_OBJECT_0);
	}

	~Mutex()
	{
		if (mutex != null)
		{
			ReleaseMutex(mutex);
			CloseHandle(mutex);
			mutex = null;
		}
	}
};

//通信用ウインドウ
class Window
{
	HWND window;

public:
	Window(WNDPROC proc, wchar* id, LPVOID data)
	{
		const wchar* wndClass = L"/tvmaid/window";

		WNDCLASS wc;
		wc.style = 0;
		wc.lpfnWndProc = proc;
		wc.cbClsExtra = 0;
		wc.cbWndExtra = 0;
		wc.hInstance = g_hinstDLL;
		wc.hIcon = null;
		wc.hCursor = null;
		wc.hbrBackground = null;
		wc.lpszMenuName = null;
		wc.lpszClassName = wndClass;

		if (GetClassInfo(g_hinstDLL, wndClass, &wc) == 0)	//クラスが登録済みか
			if (RegisterClass(&wc) == 0)
				throw Exception(Exception::CreateWindow, L"ウインドウの作成に失敗しました。");

		window = CreateWindow(wndClass, id, 0, 0, 0, 0, 0, HWND_MESSAGE, null, g_hinstDLL, data);
		if (window == null)
			throw Exception(Exception::CreateWindow, L"ウインドウの作成に失敗しました。");
	}

	~Window()
	{
		if (window != null)
		{
			DestroyWindow(window);
			window = null;
		}
	}
};

//プラグインクラスの基底クラス
//制御用ウインドウ、メモリマップ、Mutexの管理を行う
class TvmaidPluginBase : public CTVTestPlugin
{
private:
	Window* window = null;			//制御用ウインドウ
	Mutex* mutex = null;			//チューナ排他制御Mutex。このMutexは、複数のTVTest間でチューナの排他制御をするためのものなので、他で所有権を取得しないこと
	wchar driverId[MAX_PATH];		//ドライバID
	bool userStart = true;			//起動したのがユーザかどうか
	const int timeout = 60 * 1000;	//ドライバ開放待ち時間(Mutex)

protected:
	const int shareInSize = 1024;			//引数サイズ
	const int shareOutSize = 500 * 1024;	//戻り値サイズ
	SharedText* sharedIn = null;
	SharedText* sharedOut = null;

	const int streamLength = 50000;			//ストリーム長(188byte単位)
	LiveStream* liveStream = null;

private:
	//TVTestから直接呼ばれるメソッド群
	//例外を必ず処理する

	virtual bool GetPluginInfo(TVTest::PluginInfo *pInfo)
	{
		pInfo->Type = PLUGIN_TYPE_NORMAL;
		pInfo->Flags = 0;
		pInfo->pszPluginName = L"Tvmaid MAYA Plugin";
		pInfo->pszCopyright = L"(C) 2018 tvmaid project";
		pInfo->pszDescription = L"Tvmaid MAYA Plugin";

		return true;
	}

	virtual bool Initialize()
	{
		/*
		::AllocConsole();
		_wfreopen(L"CON", L"r", stdin);
		_wfreopen(L"CON", L"w", stdout);
		_wsetlocale(LC_ALL, L"japanese");
		*/
		Log(PluginVer);

		m_pApp->SetEventCallback(EventCallback, this);
		m_pApp->SetStreamCallback(0, StreamCallback, this);

		return true;
	}

	virtual bool Finalize()
	{
		Dispose();
		return true;
	}

	//TVTestストリームコールバック
	static BOOL CALLBACK StreamCallback(BYTE *pData, void *pClientData)
	{
		auto self = static_cast<TvmaidPluginBase*>(pClientData);
		if (self->liveStream != null)
			self->liveStream->Write(pData);

		return TRUE;
	}

	//TVTestイベントコールバック
	static LRESULT CALLBACK EventCallback(UINT Event, LPARAM lParam1, LPARAM lParam2, void *pClientData)
	{
		auto self = static_cast<TvmaidPluginBase*>(pClientData);

		switch (Event)
		{
		case EVENT_DRIVERCHANGE:
			//ユーザがドライバを変更した
			try
			{
				self->userStart = true;	//ユーザが起動したことにする
				self->Dispose();
				self->Init(null);
			}
			catch (Exception& e)
			{
				self->Log(e.Message);
				self->Dispose();
			}
			break;
		case EVENT_STARTUPDONE:
			self->Debug(L"Startup done");
			self->m_pApp->EnablePlugin(true);
			break;
		case EVENT_PLUGINENABLE:
			try
			{
				self->Debug(L"Enable Plugin");
				self->EnablePlugin(lParam1 != 0);
			}
			catch (Exception& e)
			{
				self->Log(e.Message);
				self->Dispose();

				//Tvmaidからの起動なら終了する
				if (self->userStart == false) { self->m_pApp->Close(); }
			}
			break;
		}

		return 0;
	}

	//通信用ウインドウコールバック
	static LRESULT CALLBACK WndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
	{
		auto self = reinterpret_cast<TvmaidPluginBase*>(GetWindowLongPtr(hwnd, GWLP_USERDATA));

		//Tvmaidからの呼び出し
		//エラーコードを返す
		if (uMsg >= 0xb000)
		{
			try
			{
				self->Call(uMsg, wParam, lParam);
				return 0;	//エラー無し
			}
			catch (Exception& e)
			{
				self->Log(e.Message);
				return e.Code;	//エラーコードを返す
			}
		}
		else
		{
			switch (uMsg)
			{
				case WM_CREATE:
				{
					auto pcs = reinterpret_cast<LPCREATESTRUCT>(lParam);
					self = reinterpret_cast<TvmaidPluginBase*>(pcs->lpCreateParams);
					SetWindowLongPtr(hwnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(self));
					return 0;	//0を返すこと
				}
			}
			return ::DefWindowProc(hwnd, uMsg, wParam, lParam);
		}
	}
	//ここまでTVTestメソッド

protected:
	virtual void Call(UINT method, WPARAM arg1, LPARAM arg2) = 0;

	void Log(wchar* msg)
	{
		m_pApp->AddLog(msg);
	}

	void Debug(wchar* msg)
	{
#ifdef _DEBUG
		m_pApp->AddLog(msg);
#endif
	}

	//ドライバ名を取得
	void GetDriver(wchar* driver)
	{
		wchar path[MAX_PATH];
		wchar ext[MAX_PATH];

		int len = m_pApp->GetDriverFullPathName(path, MAX_PATH);
		driver[0] = 0;

		//BONドライバがないときがある、その場合はドライバ名は""
		if (len > 0)
		{
			_wsplitpath_s(path, null, 0, null, 0, driver, MAX_PATH, ext, MAX_PATH);
			wcscat_s(driver, MAX_PATH, ext);
		}
	}

private:
	void EnablePlugin(bool enable)
	{
		if (enable)
		{
			//環境変数で渡されている場合は、そのDriverIdを使う
			wchar env[MAX_PATH];
			size_t size;
			_wgetenv_s(&size, null, 0, L"DriverId");

			if (size == 0)
			{
				Debug(L"ユーザが起動しました。");
				userStart = true;
				Init(null);
			}
			else
			{
				size = MAX_PATH;
				if (_wgetenv_s(&size, env, size, L"DriverId") != 0)
					throw Exception(Exception::GetEnv, L"環境変数の取得に失敗しました。");
				else
				{
					Debug(L"Tvmaidから起動しました。");
					userStart = false;
					Init(env);
				}
			}
		}
		else
			Dispose();
	}

	void Init(wchar* id)
	{
		id == null ? InitMutex() : InitMutex(id);

		wchar name[MAX_PATH];

		swprintf_s(name, L"/tvmaid/shared/in/%s", driverId);
		sharedIn = new SharedText(name, shareInSize);

		swprintf_s(name, L"/tvmaid/shared/out/%s", driverId);
		sharedOut = new SharedText(name, shareOutSize);

		swprintf_s(name, L"/tvmaid/shared/stream/%s", driverId);
		liveStream = new LiveStream(name, streamLength);

		window = new Window(WndProc, driverId, this);	//ウインドウをメモリマップより後に作る。これで通信可能か判断しているため

		wchar msg[MAX_PATH];
		wcscpy_s(msg, L"Tvmaid DriverId: ");
		wcscat_s(msg, driverId);
		Log(msg);
	}

	void Dispose()
	{
		if (window != null)
		{
			delete window;
			window = null;
		}
		if (sharedIn != null)
		{
			delete sharedIn;
			sharedIn = null;
		}
		if (sharedOut != null)
		{
			delete sharedOut;
			sharedOut = null;
		}
		if (liveStream != null)
		{
			delete liveStream;
			liveStream = null;
		}
		if (mutex != null)
		{
			delete mutex;
			mutex = null;
		}
	}

	void InitMutex(wchar* id)
	{
		wchar name[MAX_PATH];
		swprintf_s(name, L"/tvmaid/mutex/%s", id);

		mutex = new Mutex(name);
		if (mutex->GetOwner(timeout))
			wcscpy_s(driverId, id);
		else
			throw Exception(Exception::CreateMutex, L"ミューテックスの作成に失敗しました。");
	}

	void InitMutex()
	{
		wchar driver[MAX_PATH];
		driver[0] = 0;
		GetDriver(driver);

		int i = 0;
		while (true)
		{
			wchar name[MAX_PATH];

			swprintf_s(driverId, L"%s/%d", driver, i);
			swprintf_s(name, L"/tvmaid/mutex/%s", driverId);

			mutex = new Mutex(name);
			if (mutex->GetOwner(0))
				break;
			else
				i++;
		}
	}
};
