#pragma once

#include "TvmaidPluginBase.h"

//�v���O�C���N���X
class TvmaidPlugin : public TvmaidPluginBase
{
	virtual void Call(UINT api, WPARAM arg1, LPARAM arg2)
	{
		void(TvmaidPlugin::*call[])() =
		{
			&TvmaidPlugin::Close,
			&TvmaidPlugin::GetState,
			&TvmaidPlugin::GetServices,
			&TvmaidPlugin::GetEvents,
			&TvmaidPlugin::SetService,
			&TvmaidPlugin::StartRec,
			&TvmaidPlugin::StopRec,
			&TvmaidPlugin::GetEventTime,
			&TvmaidPlugin::GetTsStatus,
			&TvmaidPlugin::GetLogo
		};

		sharedOut->Init();
		(this->*call[api - 0xb000])();
	}

	//���S�擾
	void GetLogo()
	{
		HBITMAP bmp = null;
		try
		{
			int nid, tsid, sid;
			wchar path[MAX_PATH];
			swscanf_s(sharedIn->Read(), L"%d\x1" L"%d\x1" L"%d\x1" L"%[^\n]", &nid, &tsid, &sid, path, MAX_PATH);

			bmp = m_pApp->GetLogo(nid, sid, 5);
			if (bmp == null) throw 1;

			DIBSECTION dib;
			int ret = GetObject(bmp, sizeof(dib), &dib);
			if (ret == 0) throw 2;

			if (dib.dsBmih.biBitCount < 24 || dib.dsBmih.biCompression != BI_RGB) throw 3;

			FILE* fp;
			errno_t err = _wfopen_s(&fp, path, L"wb");
			if (err != 0) throw 4;

			BITMAPFILEHEADER bfh;
			bfh.bfType = 0x4d42;
			bfh.bfReserved1 = 0;
			bfh.bfReserved2 = 0;
			bfh.bfOffBits = sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER);
			int imageSize = dib.dsBm.bmWidthBytes * dib.dsBm.bmHeight;
			bfh.bfSize = bfh.bfOffBits + imageSize;

			if (fwrite(&bfh, sizeof(BITMAPFILEHEADER), 1, fp) != 1) throw 5;
			if (fwrite(&(dib.dsBmih), sizeof(BITMAPINFOHEADER), 1, fp) != 1) throw 5;
			if (fwrite(dib.dsBm.bmBits, 1, imageSize, fp) != imageSize) throw 5;
			fclose(fp);
			sharedOut->Write(L"%d", 0);
		}
		catch (int err)
		{
			sharedOut->Write(L"%d", err);
		}

		if (bmp != null) DeleteObject(bmp);
	}

	void StartRec()
	{
		RecordInfo info;
		info.Mask = RECORD_MASK_FILENAME;
		info.Flags = 0;
		info.StartTimeSpec = RECORD_START_NOTSPECIFIED;
		info.StopTimeSpec = RECORD_STOP_NOTSPECIFIED;
		info.pszFileName = sharedIn->Read();

		if (m_pApp->StartRecord(&info) == false)
			throw Exception(Exception::StartRec, L"�^����J�n�ł��܂���B");
	}

	void StopRec()
	{
		if (m_pApp->StopRecord() == false)
			throw Exception(Exception::StopRec, L"�^����~�ł��܂���B");
	}

	void Close()
	{
		if (m_pApp->Close(CLOSE_EXIT) == false)
			throw Exception(Exception::StopRec, L"TVTest�̏I���Ɏ��s���܂����B");
	}

	//�T�[�r�X�̃��X�g���擾����
	void GetServices()
	{
		int num = 0;
		m_pApp->GetTuningSpace(&num);

		// �S�Ẵ`���[�j���O��Ԃ̃T�[�r�X���擾����
		for (int space = 0; space < num; space++)
		{
			ChannelInfo info;
			info.Size = sizeof(info);

			for (int ch = 0; m_pApp->GetChannelInfo(space, ch, &info); ch++)
			{
				//�L���ȃT�[�r�X�����擾
				if (info.Flags != CHANNEL_FLAG_DISABLED)
				{
					sharedOut->Write(L"%d\x2" L"%d\x2" L"%d\x2" L"%s\x1",
						info.NetworkID,
						info.TransportStreamID,
						info.ServiceID,
						info.szChannelName);
				}
			}
		}
	}

	//�T�[�r�X�ύX
	void SetService()
	{
		int nid, tsid, sid;
		swscanf_s(sharedIn->Read(), L"%d\x1" L"%d\x1" L"%d", &nid, &tsid, &sid);

		int num = 0;
		m_pApp->GetTuningSpace(&num);

		for (int space = 0; space < num; space++)
		{
			ChannelInfo ci;
			ci.Size = sizeof(ci);

			for (int ch = 0; m_pApp->GetChannelInfo(space, ch, &ci); ch++)
			{
				if (ci.Flags != CHANNEL_FLAG_DISABLED && ci.NetworkID == nid && ci.TransportStreamID == tsid && ci.ServiceID == sid)
				{
					if (m_pApp->SetChannel(space, ch, sid) == false)
						throw Exception(Exception::SetService, L"�T�[�r�X�̕ύX�Ɏ��s���܂����B");

					//�ő�30 * 100ms�҂�
					for (int i = 0; i < 30; i++)
					{
						Sleep(100);
						int index = m_pApp->GetService();	//�T�[�r�X�ύX����-1���Ԃ�
						if (index != -1)
						{
							ServiceInfo si;
							si.Size = sizeof(ServiceInfo);
							m_pApp->GetServiceInfo(index, &si);
							if (sid == si.ServiceID)
								return;
							else
								break;
						}
					}
					throw Exception(Exception::SetService, L"�T�[�r�X�̕ύX�Ɏ��s���܂����B");
				}
			}
		}
		throw Exception(Exception::SetService, L"�T�[�r�X�̕ύX�Ɏ��s���܂����B");
	}

	void GetTimeStr(SYSTEMTIME* time, wchar* buf, size_t size)
	{
		swprintf_s(buf, size, L"%d/%d/%d %02d:%02d:%02d",
			time->wYear,
			time->wMonth,
			time->wDay,
			time->wHour,
			time->wMinute,
			time->wSecond);
	}

	//�w��ԑg�̎��Ԃ��擾(�^��p���A���ԃ`�F�b�N�p)
	void GetEventTime()
	{
		int nid, tsid, sid, eid;
		swscanf_s(sharedIn->Read(), L"%d\x1" L"%d\x1" L"%d\x1" L"%d", &nid, &tsid, &sid, &eid);

		EpgEventQueryInfo info;
		info.NetworkID = nid;
		info.TransportStreamID = tsid;
		info.ServiceID = sid;
		info.EventID = eid;
		info.Type = EPG_EVENT_QUERY_EVENTID;
		info.Flags = 0;
		EpgEventInfo* event = m_pApp->GetEpgEventInfo(&info);

		if (event != null)
		{
			wchar time[30];
			GetTimeStr(&event->StartTime, time, sizeof(time) / sizeof(wchar_t));
			sharedOut->Write(L"%s\x1%d", time, event->Duration);
			m_pApp->FreeEpgEventInfo(event);
		}
		else
			throw Exception(Exception::GetEventTime, L"�ԑg���Ԃ̎擾�Ɏ��s���܂����B");
	}

	void GetEvents()
	{
		int nid, tsid, sid;
		swscanf_s(sharedIn->Read(), L"%d\x1" L"%d\x1" L"%d", &nid, &tsid, &sid);

		EpgEventList list;
		list.NetworkID = nid;
		list.TransportStreamID = tsid;
		list.ServiceID = sid;

		if (m_pApp->GetEpgEventList(&list))
		{
			for (int i = 0; i < list.NumEvents; i++)
			{
				EpgEventInfo* event = list.EventList[i];

				wchar time[30];
				GetTimeStr(&event->StartTime, time, sizeof(time) / sizeof(wchar_t));
				
				wchar* nullStr = L"";
				sharedOut->Write(L"%d\x2" L"%s\x2" L"%d\x2" L"%s\x2" L"%s\x2" L"%s\x2" L"%u\x2" L"%d\x1",
					event->EventID,
					time,
					event->Duration,
					event->pszEventName == null ? nullStr : event->pszEventName,
					event->pszEventText == null ? nullStr : event->pszEventText,
					event->pszEventExtendedText == null ? nullStr : event->pszEventExtendedText,
					GetGenre(event),
					event->FreeCaMode);
			}
			m_pApp->FreeEpgEventList(&list);
		}
		else
			throw Exception(Exception::GetEvents, L"�ԑg�\�̎擾�Ɏ��s���܂����B");
	}

	//�W���������擾�B�ő�4�܂Ŏ擾����
	//�W�������̐��𑝂₷�ꍇ�́Aint64�ɕύX���邱��
	//sharedOut.Write�̏������ύX���邱��
	uint GetGenre(EpgEventInfo* event)
	{
		uint data = 0;

		for (int i = 0; i < 4; i++)
		{
			if (i < event->ContentListLength)
			{
				EpgEventContentInfo* info = event->ContentList + i;
				uint level1 = info->ContentNibbleLevel1;
				uint level2 = info->ContentNibbleLevel2;
				uint genre = (level1 << 4) + level2;
				data += genre << (i * 8);
			}
			else
				data += 0xff << (i * 8);
		}

		return data;
	}

	void GetState()
	{
		RecordStatusInfo info;
		info.Size = sizeof(RecordStatusInfo);

		if (m_pApp->GetRecordStatus(&info))
			sharedOut->Write(L"%d", info.Status);
		else
			throw Exception(Exception::GetState, L"��Ԃ̎擾�Ɏ��s���܂����B");
	}

	void GetTsStatus()
	{
		StatusInfo info;
		info.Size = sizeof(StatusInfo);

		if (m_pApp->GetStatus(&info))
			sharedOut->Write(L"%d\x1" L"%d\x1" L"%d", info.ErrorPacketCount, info.DropPacketCount, info.ScramblePacketCount);
		else
			throw Exception(Exception::GetTsStatus, L"TS��Ԃ̎擾�Ɏ��s���܂����B");
	}
};
