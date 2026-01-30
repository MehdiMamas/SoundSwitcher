#include "Includes.h"

// version that handles its own COM initialization (for single calls)
HRESULT SetAudioPlaybackDevice(const LPCWSTR devID, ERole role)
{
		HRESULT hr;

		hr = CoInitialize(nullptr);

		IPolicyConfigVista* pPolicyConfig;

		hr = CoCreateInstance(__uuidof(CPolicyConfigVistaClient), nullptr, CLSCTX_ALL, __uuidof(IPolicyConfigVista), reinterpret_cast<LPVOID*>(&pPolicyConfig));

		if (SUCCEEDED(hr))
		{
				hr = pPolicyConfig->SetDefaultEndpoint(devID, role);
				pPolicyConfig->Release();
		}

		CoUninitialize();

		return hr;
}

// version for setting both roles - uses separate COM sessions with delay
void SetAudioPlaybackDeviceBoth(const LPCWSTR devID)
{
		FILE* logFile = nullptr;
		fopen_s(&logFile, "SetPlaybackDevice.log", "a");
		if (logFile) {
			fprintf(logFile, "SetAudioPlaybackDeviceBoth called\n");
			fwprintf(logFile, L"DeviceID: %s\n", devID);
		}

		// set console device
		HRESULT hr1 = SetAudioPlaybackDevice(devID, eConsole);
		if (logFile) fprintf(logFile, "SetDefaultEndpoint(eConsole) hr: 0x%08X\n", hr1);
		
		// delay to let Windows process the first change
		Sleep(100);
		
		// set communications device
		HRESULT hr2 = SetAudioPlaybackDevice(devID, eCommunications);
		if (logFile) fprintf(logFile, "SetDefaultEndpoint(eCommunications) hr: 0x%08X\n", hr2);
		
		if (logFile) {
			fprintf(logFile, "---\n");
			fclose(logFile);
		}
}
