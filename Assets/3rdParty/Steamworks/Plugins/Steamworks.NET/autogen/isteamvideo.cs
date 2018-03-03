// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2017 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is automatically generated.
// Changes to this file will be reverted when you update Steamworks.NET

#if !DISABLESTEAMWORKS

using System.Runtime.InteropServices;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.types.SteamTypes;
using IntPtr = System.IntPtr;

namespace _3rdParty.Steamworks.Plugins.Steamworks.NET.autogen {
	public static class SteamVideo {
		/// <summary>
		/// <para> Get a URL suitable for streaming the given Video app ID's video</para>
		/// </summary>
		public static void GetVideoURL(AppId_t unVideoAppID) {
			InteropHelp.TestIfAvailableClient();
			NativeMethods.ISteamVideo_GetVideoURL(CSteamAPIContext.GetSteamVideo(), unVideoAppID);
		}

		/// <summary>
		/// <para> returns true if user is uploading a live broadcast</para>
		/// </summary>
		public static bool IsBroadcasting(out int pnNumViewers) {
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamVideo_IsBroadcasting(CSteamAPIContext.GetSteamVideo(), out pnNumViewers);
		}

		/// <summary>
		/// <para> Get the OPF Details for 360 Video Playback</para>
		/// </summary>
		public static void GetOPFSettings(AppId_t unVideoAppID) {
			InteropHelp.TestIfAvailableClient();
			NativeMethods.ISteamVideo_GetOPFSettings(CSteamAPIContext.GetSteamVideo(), unVideoAppID);
		}

		public static bool GetOPFStringForApp(AppId_t unVideoAppID, out string pchBuffer, ref int pnBufferSize) {
			InteropHelp.TestIfAvailableClient();
			IntPtr pchBuffer2 = Marshal.AllocHGlobal((int)pnBufferSize);
			bool ret = NativeMethods.ISteamVideo_GetOPFStringForApp(CSteamAPIContext.GetSteamVideo(), unVideoAppID, pchBuffer2, ref pnBufferSize);
			pchBuffer = ret ? InteropHelp.PtrToStringUTF8(pchBuffer2) : null;
			Marshal.FreeHGlobal(pchBuffer2);
			return ret;
		}
	}
}

#endif // !DISABLESTEAMWORKS
