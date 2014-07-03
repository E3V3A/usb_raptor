Module ModPublicVals
    Public EggCount As Integer = 5


    Public MyID As String
    Public MyPassword As String
    Public AllowDelay As Boolean
    Public AllowCancelDuringDelay As Boolean
    Public EncryptionKey As String = "Default"
    Public EnableUnlockWithEncryptionKey As Boolean = False
    Public LockOnUSBSerialNumber As Boolean = False
    Public SerialNumberWhileLock As String
    Public EnableMasterKeyBackDoor As Boolean = True
    Public EnableNetworkBackDoor As Boolean
    Public MuteWhileLock As Boolean
    Public ShowMuteControl As Boolean
    Public MakeSoundWhenlocking As Boolean
    Public LockEnd As Boolean = False
    Public RandomLockScreenColor As Boolean = True
    Public KeepRandomizing As Boolean = False
    Public SyncColorAcrossNet As Boolean
    Public RemoteColor As Color

    Public PasswordProtected As Boolean = False
    Public CommandIsExit As Boolean = False
    Public CommandIsEnableDisable As Boolean = False

    Public ShowArmNotifications As Boolean = True
    Public ReArmNotification As Boolean = True

    Public FirstTimeRun As Boolean = False
    Public FileSuccessfullyCreated As Boolean = False

    Public AllowLockdown As Boolean = False

    Public ForceLockCommand As Boolean = False
End Module
