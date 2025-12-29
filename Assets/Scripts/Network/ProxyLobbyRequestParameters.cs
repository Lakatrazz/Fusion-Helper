using LiteNetLib.Utils;

namespace FusionHelper
{
    public struct ProxyLobbyRequestParameters
    {
        public MatchmakerFilters Filters;

        public int VersionMajor;

        public int VersionMinor;

        public string LobbyCode;

        public static ProxyLobbyRequestParameters Read(NetDataReader reader)
        {
            bool filterFull = reader.GetBool();
            bool filterMismatchingVersions = reader.GetBool();

            int versionMajor = reader.GetInt();
            int versionMinor = reader.GetInt();

            bool hasCode = reader.GetBool();
            string lobbyCode = null;

            if (hasCode)
            {
                lobbyCode = reader.GetString();
            }

            var parameters = new ProxyLobbyRequestParameters
            {
                Filters = new MatchmakerFilters()
                {
                    FilterFull = filterFull,
                    FilterMismatchingVersions = filterMismatchingVersions,
                },
                VersionMajor = versionMajor,
                VersionMinor = versionMinor,
                LobbyCode = lobbyCode,
            };

            return parameters;
        }
    }
}