using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NET.Tools.Terminal
{
    public enum TerminalConfigMode
    {
        ConfigMode,
        NonConfigMode,
        InterfaceConfig,
        VlanDatabaseConfig
    }

    public enum TerminalProtocol
    {
        //Default = -1,
        Telnet = 0,
        SSH1 = 1,
        SSH2 = 2
    }

    public enum TelnetProviderType
    {
        //Default = 1,
        //SimpleTelnet = 0,
        TelnetPipeClient = 0,
        TelnetSocketClient = 1,
        ThoughtNetTelnet = 2
        //IPWorks = 1
    }

    public enum Ssh2ProviderType
    {
        //Default = -1,
        SshNet = 0,
    }

    public enum Ssh1ProviderType
    {
        //Default = -1,
        SshGranados = 0
    }

    public enum TerminalConnectionState
    {
        Authenticated,
        Disconnected,
        LoginInProgressWaitingForUsernamePrompt,
        LoginInProgressWaitingForPasswordPrompt,
        LoginInProgressWaitingForEnableSecretPrompt,
        LoginInProgressWaitingForEnableSecretPasswordPrompt,
        LoginInProgressAfterEnableSecretIsSent,
        LoginInProgressReadyToBeConnected
    }
}
