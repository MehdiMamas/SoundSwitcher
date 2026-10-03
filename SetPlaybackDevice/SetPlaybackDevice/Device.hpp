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

// sets console, multimedia, and communications in one COM session
void SetAudioPlaybackDeviceBoth(const LPCWSTR devID)
{
		HRESULT hr = CoInitialize(nullptr);
		if (FAILED(hr) && hr != RPC_E_CHANGED_MODE)
		{
				return;
		}

		IPolicyConfigVista* pPolicyConfig = nullptr;
		hr = CoCreateInstance(__uuidof(CPolicyConfigVistaClient), nullptr, CLSCTX_ALL, __uuidof(IPolicyConfigVista), reinterpret_cast<LPVOID*>(&pPolicyConfig));

		if (SUCCEEDED(hr))
		{
				pPolicyConfig->SetDefaultEndpoint(devID, eConsole);
				pPolicyConfig->SetDefaultEndpoint(devID, eMultimedia);
				pPolicyConfig->SetDefaultEndpoint(devID, eCommunications);
				pPolicyConfig->Release();
		}

		CoUninitialize();
}
