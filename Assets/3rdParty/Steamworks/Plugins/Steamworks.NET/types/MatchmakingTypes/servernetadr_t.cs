// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2017 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

#if !DISABLESTEAMWORKS

namespace _3rdParty.Steamworks.Plugins.Steamworks.NET.types.MatchmakingTypes {
	// servernetadr_t is all the addressing info the serverbrowser needs to know about a game server,
	// namely: its IP, its connection port, and its query port.
	[System.Serializable]
	public struct servernetadr_t {
		private ushort m_usConnectionPort;	// (in HOST byte order)
		private ushort m_usQueryPort;
		private uint m_unIP;

		public void Init(uint ip, ushort usQueryPort, ushort usConnectionPort) {
			m_unIP = ip;
			m_usQueryPort = usQueryPort;
			m_usConnectionPort = usConnectionPort;
		}

#if NETADR_H
		public netadr_t GetIPAndQueryPort() {
			return netadr_t( m_unIP, m_usQueryPort );
		}
#endif

		// Access the query port.
		public ushort GetQueryPort() {
			return m_usQueryPort;
		}

		public void SetQueryPort(ushort usPort) {
			m_usQueryPort = usPort;
		}

		// Access the connection port.
		public ushort GetConnectionPort() {
			return m_usConnectionPort;
		}

		public void SetConnectionPort(ushort usPort) {
			m_usConnectionPort = usPort;
		}

		// Access the IP
		public uint GetIP() {
			return m_unIP;
		}

		public void SetIP(uint unIP) {
			m_unIP = unIP;
		}

		// This gets the 'a.b.c.d:port' string with the connection port (instead of the query port).
		public string GetConnectionAddressString() {
			return ToString(m_unIP, m_usConnectionPort);
		}

		public string GetQueryAddressString() {
			return ToString(m_unIP, m_usQueryPort);
		}

		public static string ToString(uint unIP, ushort usPort) {
#if VALVE_BIG_ENDIAN
		return string.Format("{0}.{1}.{2}.{3}:{4}", unIP & 0xFFul, (unIP >> 8) & 0xFFul, (unIP >> 16) & 0xFFul, (unIP >> 24) & 0xFFul, usPort);
#else
		return string.Format("{0}.{1}.{2}.{3}:{4}", (unIP >> 24) & 0xFFul, (unIP >> 16) & 0xFFul, (unIP >> 8) & 0xFFul, unIP & 0xFFul, usPort);
#endif
		}

		public static bool operator <(servernetadr_t x, servernetadr_t y) {
			return (x.m_unIP < y.m_unIP) || (x.m_unIP == y.m_unIP && x.m_usQueryPort < y.m_usQueryPort);
		}

		public static bool operator >(servernetadr_t x, servernetadr_t y) {
			return (x.m_unIP > y.m_unIP) || (x.m_unIP == y.m_unIP && x.m_usQueryPort > y.m_usQueryPort);
		}

		public override bool Equals(object other) {
			return other is servernetadr_t && this == (servernetadr_t)other;
		}

		public override int GetHashCode() {
			return m_unIP.GetHashCode() + m_usQueryPort.GetHashCode() + m_usConnectionPort.GetHashCode();
		}

		public static bool operator ==(servernetadr_t x, servernetadr_t y) {
			return (x.m_unIP == y.m_unIP) && (x.m_usQueryPort == y.m_usQueryPort) && (x.m_usConnectionPort == y.m_usConnectionPort);
		}

		public static bool operator !=(servernetadr_t x, servernetadr_t y) {
			return !(x == y);
		}

		public bool Equals(servernetadr_t other) {
			return (m_unIP == other.m_unIP) && (m_usQueryPort == other.m_usQueryPort) && (m_usConnectionPort == other.m_usConnectionPort);
		}

		public int CompareTo(servernetadr_t other) {
			return m_unIP.CompareTo(other.m_unIP) + m_usQueryPort.CompareTo(other.m_usQueryPort) + m_usConnectionPort.CompareTo(other.m_usConnectionPort);
		}
	}
}

#endif // !DISABLESTEAMWORKS
