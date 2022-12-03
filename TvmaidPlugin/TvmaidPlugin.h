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
			&TvmaidPlugin::GetLogo,
			&TvmaidPlugin::IsEpgComplete
		};

		sharedOut->Init();
		(this->*call[api - 0xb000])();
	}

	//�߂�l: 0:����,1=����,-1:�G���[
	int GetEpgStatus(int nid, int tsid)
	{
		if (m_pApp->QueryMessage(MESSAGE_GETEPGCAPTURESTATUS) == false)
			return -1;

		auto status = tsid == 0 ? EPG_CAPTURE_STATUS_SCHEDULEBASICCOMPLETED : EPG_CAPTURE_STATUS_SCHEDULEBASICCOMPLETED | EPG_CAPTURE_STATUS_SCHEDULEEXTENDEDCOMPLETED;

		int count = 0;
		m_pApp->GetTuningSpace(&count);

		for (int space = 0; space < count; space++)
		{
			ChannelInfo chInfo;
			chInfo.Size = sizeof(chInfo);

			for (int ch = 0; m_pApp->GetChannelInfo(space, ch, &chInfo); ch++)
			{
				if (chInfo.Flags & CHANNEL_FLAG_DISABLED)
					continue;

				if (nid == chInfo.NetworkID && (tsid == 0 || tsid == chInfo.TransportStreamID))
				{
					EpgCaptureStatusInfo epgInfo;
					epgInfo.Size = sizeof(epgInfo);
					epgInfo.Flags = 0;
					epgInfo.NetworkID = chInfo.NetworkID;
					epgInfo.TransportStreamID = chInfo.TransportStreamID;
					epgInfo.ServiceID = chInfo.ServiceID;
					epgInfo.Status = status;

					if (m_pApp->GetEpgCaptureStatus(&epgInfo) == false)
						return -1;

					if ((epgInfo.Status & status) != status)
						return 0;
				}
			}
		}
		return 1;
	}

	//�ԑg�\�擾�������������ǂ���
	//�߂�l:����=1, �����܂��̓G���[=0
	//����:tsid=0     �w��NID�̃T�[�r�X�Ŋ�{�����擾����������
	//     tsid=0�ȊO �w��NID,TSID�̃T�[�r�X�Ŋg�������擾����������
	void IsEpgComplete()
	{
		int nid, tsid;
		swscanf_s(sharedIn->Read(), L"%d\x1" L"%d", &nid, &tsid);

		auto ret = GetEpgStatus(nid, tsid);
		sharedOut->Write(L"%d", ret == 1 ? 1 : 0);
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
				if ((info.Flags & CHANNEL_FLAG_DISABLED) == 0)
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
				if ((ci.Flags & CHANNEL_FLAG_DISABLED) == 0 && ci.NetworkID == nid && ci.TransportStreamID == tsid && ci.ServiceID == sid)
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
				sharedOut->Write(L"%d\x2" L"%s\x2" L"%d\x2" L"%s\x2" L"%s\x2" L"%s\x2" L"%lld\x2" L"%d\x1",
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

	//�W���������擾�B�ő�6�܂Ŏ擾����
	int64 GetGenre(EpgEventInfo* event)
	{
		int64 ret = 0;

		for (int i = 0; i < 6; i++)
		{
			if (i < event->ContentListLength)
			{
				auto info = event->ContentList + i;
				int64 genre = ((int)info->ContentNibbleLevel1 << 4) + info->ContentNibbleLevel2;

				//�g���W���������W�������փ}�b�s���O
				if (genre == 0xe0 && info->UserNibble1 == 0 && info->UserNibble2 <= 0x5)
					genre = 0xf0 + info->UserNibble2;	//�ԑg�t�����->0xf0�`0xf5
				else if (genre == 0xe0 && info->UserNibble1 == 1 && info->UserNibble2 <= 0x1)
					genre = 0xfa + info->UserNibble2;	//�ԑg�t�����->0xfa�`0xfb
				else if (genre == 0xe1 && info->UserNibble1 == 0)
					genre = 0xc0 + info->UserNibble2;	//CS�X�|�[�c->0xc0�`0xcf
				else if (genre == 0xe1 && info->UserNibble1 == 1)
					genre = 0xd0 + info->UserNibble2;	//CS�m��->0xd0�`0xdf
				else if (genre == 0xe1 && info->UserNibble1 == 2 && info->UserNibble2 == 0x6)
					genre = 0xdd;						//CS�M��0x6->0xdd
				else if (genre == 0xe1 && info->UserNibble1 == 2 && info->UserNibble2 == 0x7)
					genre = 0xde;						//CS�M��0x7->0xde
				else if (genre == 0xe1 && info->UserNibble1 == 2)
					genre = 0xd0 + info->UserNibble2;	//CS�M��0x6,0x7�ȊO->0xd0�`0xdf
			
				ret += genre << (i * 8);
			}
			else
			{
				int64 genre = 0xff;
				ret += genre << (i * 8);
			}
		}

		return ret;
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
