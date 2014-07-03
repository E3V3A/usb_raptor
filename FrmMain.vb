Imports System.IO
Imports System

Imports System.Security
Imports System.Security.Cryptography
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports System.Text
Imports Microsoft.Win32
Imports System.Net

Imports Scripting


'Imports System.Text.Encoding


Public Class FrmMain



    Private Declare Function LockWorkStation Lib "user32.dll" () As Integer
    Dim UnlockFileFound As Boolean = False
    Dim FilePath As String = ""
    Dim TextFromFile As String = ""
    Dim UnlockText

    Dim MasterPresent As Boolean = False

    Dim UnlockDrive As String
    Dim SerialNumber As String

    Dim BroadcastTarget1 = "255.255.255.255"
    Dim BroadcastTarget2 = Nothing
    Dim BroadcastTarget3 = Nothing

    Dim NumberOfDrives As Integer = 0
    Dim WaitToUnlock As Boolean = False
    Dim StartInTray As Boolean

    Dim numberofmonitors As Integer

    Private Const WM_DEVICECHANGE As Integer = &H219
    Private Const DBT_DEVICEARRIVAL As Integer = &H8000
    Private Const DBT_DEVTYP_VOLUME As Integer = &H2

    'Device information structure
    Public Structure DEV_BROADCAST_HDR
        Public dbch_size As Int32
        Public dbch_devicetype As Int32
        Public dbch_reserved As Int32
    End Structure

    'Volume information Structure
    Private Structure DEV_BROADCAST_VOLUME
        Public dbcv_size As Int32
        Public dbcv_devicetype As Int32
        Public dbcv_reserved As Int32
        Public dbcv_unitmask As Int32
        Public dbcv_flags As Int16
    End Structure

    'Function that gets the drive letter from the unit mask
    Private Function GetDriveLetterFromMask(ByRef Unit As Int32) As Char
        Dim i As Integer
        For i = 0 To 25
            If CBool(Unit And i) Then Exit For
            Unit = Unit >> 1
        Next
        Return Chr(i + 1 + Asc("A"))
    End Function



#Region "QueryCancelAutoPlay"
    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
    Private Shared Function RegisterWindowMessage(ByVal lpString As String) As UInteger
    End Function

    ''provide a private internal message id
    Private queryCancelAutoPlay As UInt32 = RegisterWindowMessage("QueryCancelAutoPlay")
    Private CancelAutoplay As New IntPtr(0) 'IntPtr(0) is False IntPtr(1) is True

    'Protected Sub WndProc(ByRef m As Message)
    '    'Call the base first, don't want my result value to be overwritten
    '    MyBase.WndProc(m)

    '    'Incase QueryCancelAutoPlay was not registered...
    '    If queryCancelAutoPlay = 0 Then
    '        queryCancelAutoPlay = RegisterWindowMessage("QueryCancelAutoPlay")
    '    End If

    '    'If m.msg equals the QueryCancelAutoPlay then cancel it.
    '    If Convert.ToUInt32(m.Msg) = queryCancelAutoPlay Then
    '        m.Result = CancelAutoplay
    '    End If
    'End Sub
#End Region





    'Override message processing to check for the DEVICECHANGE message
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        If m.Msg = WM_DEVICECHANGE Then
            If CInt(m.WParam) = DBT_DEVICEARRIVAL Then
                Dim DeviceInfo As DEV_BROADCAST_HDR
                DeviceInfo = DirectCast(Marshal.PtrToStructure(m.LParam, GetType(DEV_BROADCAST_HDR)), DEV_BROADCAST_HDR)
                If DeviceInfo.dbch_devicetype = DBT_DEVTYP_VOLUME Then
                    Dim Volume As DEV_BROADCAST_VOLUME
                    Volume = DirectCast(Marshal.PtrToStructure(m.LParam, GetType(DEV_BROADCAST_VOLUME)), DEV_BROADCAST_VOLUME)
                    Dim DriveLetter As String = (GetDriveLetterFromMask(Volume.dbcv_unitmask) & ":\")

                    If IO.File.Exists(IO.Path.Combine(DriveLetter, "unlock.k3y")) Then
                        '<<<< The test file has been found >>>>
                        StLbl2.Text = "New Drive Plugged: " & IO.Path.Combine(DriveLetter)
                        '    Me.Text = ("Found unlock file")
                        'Armed = True
                        UnlockFileFound = True

                    Else
                        '<<<< Test file has not been found >>>>
                        '   Me.Text = ("Could not find unlock file")
                        UnlockFileFound = False
                    End If


                End If
            End If
            UnlockFileFound = False
            LoadDrives()

        End If
        MyBase.WndProc(m)




        ''provide a private internal message id
        'Private queryCancelAutoPlay As UInt32 = RegisterWindowMessage("QueryCancelAutoPlay")
        'Private CancelAutoplay As New IntPtr(0) 'IntPtr(0) is False IntPtr(1) is True

        'Call the base first, don't want my result value to be overwritten
        '    MyBase.WndProc(m)

        'Incase QueryCancelAutoPlay was not registered...
        If queryCancelAutoPlay = 0 Then
            queryCancelAutoPlay = RegisterWindowMessage("QueryCancelAutoPlay")
        End If

        'If m.msg equals the QueryCancelAutoPlay then cancel it.
        If Convert.ToUInt32(m.Msg) = queryCancelAutoPlay Then
            m.Result = CancelAutoplay
        End If

    End Sub



#Region "UDP Stuff"
    Dim publisher As New Sockets.UdpClient(0)
    Dim subscriber As New Sockets.UdpClient(0)
    'Dim subscriber As New Sockets.UdpClient(39712)
    Dim IndicatorToggle As Integer = 0
    Dim ReceivedText As String
#End Region





    Sub CheckDrives()
        LblINd.BackColor = Color.White
        NtIcn.Icon = Me.Icon
        Dim totaldrives As Integer = 0
        Dim allDrives() As DriveInfo = DriveInfo.GetDrives()

        Dim d As DriveInfo
        For Each d In allDrives
            'stlbl2.text += (d.Name)
            ' stlbl2.text += (" " & d.DriveType & " ") ' 2 type is removable
            If d.DriveType = 2 Then
                If d.Name <> "A:\" And d.Name <> "B:\" Then 'trap floppies here for old systems where delays are caused and strange behavior occurs
                    If d.IsReady = True Then

                        If IO.File.Exists(IO.Path.Combine(d.Name, "unlock.k3y")) Then

                            '<<<< The unlock file has been found >>>>
                            ' Me.Text = IO.Path.Combine(d.Name, "unlock.txt")
                            StLbl2.Text = ("Unlock drive " & d.Name)
                            UnlockDrive = d.Name
                            StLbl3.Text = ""
                            ' If ForceLockCommand = False Then
                            UnlockFileFound = True
                            '     Else
                            '   End If

                            FilePath = IO.Path.Combine(d.Name, "unlock.k3y")

                            Call ReadFile()
                            If MasterPresent Then '???? check what the value is 
                                Armed = True
                            End If

                            Exit Sub
                        Else
                            '<<<< Unlock file has not been found >>>>
                            UnlockFileFound = False
                        End If
                        If EnableMasterKeyBackDoor = True Then
                            If IO.File.Exists(IO.Path.Combine(d.Name, "master.k3y")) Then
                                FilePath = IO.Path.Combine(d.Name, "master.k3y")
                                Call ReadMasterFile()
                                'Decode master key!
                                Exit Sub
                            Else
                                MasterPresent = False
                            End If
                        End If

                        UnlockFileFound = False
                    End If
                    UnlockFileFound = False
                End If

                UnlockFileFound = False
            End If
            UnlockFileFound = False
            totaldrives += 1
        Next

        StLbl3.Text = "Drives scanned: " & totaldrives



        '   If Armed = True Then
        If UnlockFileFound = False Or ForceLockCommand = True Then
            StLbl2.Text = "File not found"
            LblSN.Text = "Not detected"
            LblINd.BackColor = Color.Red
            NtIcn.Icon = My.Resources.RaptorClaws_New_Red_Tray

            If AllowDelay = False Then
                Call LockSeq()
            Else
                If FrmMonitor1.Visible = False And Armed = True Then
                    If CheckBox3.Checked = True Or CheckBox5.Checked = True Then
                        FrmNumbers.Show()
                    End If

                End If

            End If

        End If
        '  Else
        ' LblINd.BackColor = Color.Black
        ' stlbl2.text = "Not Armed Yet"
        '    End If

    End Sub




    Sub LockSeq()
        If Armed = True Then
            If CheckBox3.Checked = True Then
                Armed = False
                LockWorkStation()
                WaitToUnlock = True

            ElseIf CheckBox5.Checked = True Then
                If FrmMonitor1.Visible = False Then 'update this value only if the computer is unlocked to avoid updat of the variable while the computer is locked 
                    SerialNumberWhileLock = SerialNumber
                    SaveSetting("Raptor", "Setting", "SerialNumberWhileLock", SerialNumberWhileLock)
                    'error was here while locking and removed the USB stick
                End If

                Call Button5_Click(Nothing, EventArgs.Empty)
                End If
        End If


    End Sub



    'Private Sub Button1_Click(sender As Object, e As EventArgs)
    '    LblINd.BackColor = Color.White
    '    CheckDrives()
    'End Sub



    Sub ReadFile()
        On Error Resume Next
        Dim tr As IO.TextReader = New IO.StreamReader(FilePath)
        TextFromFile = tr.ReadToEnd
        Dim strDecrptedText As String
        strDecrptedText = Decrypt(TextFromFile)
        tr.Dispose()
        tr.Close()
        If ForceLockCommand = True Then
            ' Avoid unlock while in lockdown mode even if teh usb key is placed
        Else

            If strDecrptedText = UnlockText Then
                'unlock sequence
                'Beep()
                StLbl3.Text = "Unlock key correct!"
                LblINd.BackColor = Color.Chartreuse
                NtIcn.Icon = My.Resources.RaptorClaws_New_Green_Tray

                GetSerialNumber()
                If LockOnUSBSerialNumber = True Then
                    If SerialNumberWhileLock = SerialNumber Then
                        CloseLockForms()
                    Else
                        ' Exit Sub
                    End If
                Else
                    CloseLockForms()
                End If


                If WaitToUnlock = True Then
                    ' SendKeys.Send("init{ENTER}")
                    ' MsgBox("snt")
                    WaitToUnlock = False
                End If
                If MasterPresent = False Then
                    Armed = True
                End If
            Else
                StLbl3.Text = "Wrong unlock key"
                LblINd.BackColor = Color.Purple
                NtIcn.Icon = My.Resources.RaptorClaws_New_Purple_Tray
                Call LockSeq()
                'Armed = False
            End If


        End If

    End Sub


    Sub ReadMasterFile()

        Dim tr As IO.TextReader = New IO.StreamReader(FilePath)
        TextFromFile = tr.ReadToEnd
        Dim strDecrptedText As String
        strDecrptedText = MasterDecrypt(TextFromFile)
        tr.Dispose()
        tr.Close()
        If strDecrptedText = MasterUnlockText Then
            'unlock sequence
            'Beep()
            ForceLockCommand = False
            StLbl3.Text = "Master key used!"

            LblINd.BackColor = Color.Chartreuse
            NtIcn.Icon = My.Resources.RaptorClaws_New_Green_Tray
            'If FrmMonitor1.Visible = True Then
            '    FrmMonitor1.Close()
            '    '  FrmMonitor2.Close()
            'End If
            'If FrmMonitor2.Visible = True Then
            '    FrmMonitor2.Close()
            'End If

            CloseLockForms()

            CheckBox1.Checked = False
            EnableToolStripMenuItem.Checked = False

            CheckBox4.Checked = False
            Armed = False
            MasterPresent = True
            'If WaitToUnlock = True Then
            '    ' SendKeys.Send("init{ENTER}")
            '    ' MsgBox("snt")
            '    WaitToUnlock = False
            'End If
            '    Armed = True
            PasswordProtected = False
            CheckBox17.Checked = False


        Else
            MasterPresent = False
            '   StLbl3.Text = "Wrong unlock key"
            '   LblINd.BackColor = Color.Purple
            '   Call LockSeq()
            '   Armed = False
        End If

    End Sub




    Public Function Encrypt(ByVal plainText As String) As String

        Dim passPhrase As String = EncryptionKey
        Dim saltValue As String = "mySaltValue"
        Dim hashAlgorithm As String = "SHA1"

        Dim passwordIterations As Integer = 2
        Dim initVector As String = "@1B2c3D4e5F6g7H8"
        Dim keySize As Integer = 256

        Dim initVectorBytes As Byte() = Encoding.ASCII.GetBytes(initVector)
        Dim saltValueBytes As Byte() = Encoding.ASCII.GetBytes(saltValue)

        Dim plainTextBytes As Byte() = Encoding.UTF8.GetBytes(plainText)


        Dim password As New PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations)

        Dim keyBytes As Byte() = password.GetBytes(keySize \ 8)
        Dim symmetricKey As New RijndaelManaged()

        symmetricKey.Mode = CipherMode.CBC

        Dim encryptor As ICryptoTransform = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes)

        Dim memoryStream As New MemoryStream()
        Dim cryptoStream As New CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)

        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length)
        cryptoStream.FlushFinalBlock()
        Dim cipherTextBytes As Byte() = memoryStream.ToArray()
        memoryStream.Close()
        cryptoStream.Close()
        Dim cipherText As String = Convert.ToBase64String(cipherTextBytes)
        Return cipherText
    End Function
    Public Function Decrypt(ByVal cipherText As String) As String

        On Error Resume Next
        Dim passPhrase As String = EncryptionKey
        Dim saltValue As String = "mySaltValue"
        Dim hashAlgorithm As String = "SHA1"

        Dim passwordIterations As Integer = 2
        Dim initVector As String = "@1B2c3D4e5F6g7H8"
        Dim keySize As Integer = 256
        ' Convert strings defining encryption key characteristics into byte
        ' arrays. Let us assume that strings only contain ASCII codes.
        ' If strings include Unicode characters, use Unicode, UTF7, or UTF8
        ' encoding.
        Dim initVectorBytes As Byte() = Encoding.ASCII.GetBytes(initVector)
        Dim saltValueBytes As Byte() = Encoding.ASCII.GetBytes(saltValue)

        ' Convert our ciphertext into a byte array.
        Dim cipherTextBytes As Byte() = Convert.FromBase64String(cipherText)

        ' First, we must create a password, from which the key will be 
        ' derived. This password will be generated from the specified 
        ' passphrase and salt value. The password will be created using
        ' the specified hash algorithm. Password creation can be done in
        ' several iterations.
        Dim password As New PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations)

        ' Use the password to generate pseudo-random bytes for the encryption
        ' key. Specify the size of the key in bytes (instead of bits).
        Dim keyBytes As Byte() = password.GetBytes(keySize \ 8)

        ' Create uninitialized Rijndael encryption object.
        Dim symmetricKey As New RijndaelManaged()

        ' It is reasonable to set encryption mode to Cipher Block Chaining
        ' (CBC). Use default options for other symmetric key parameters.
        symmetricKey.Mode = CipherMode.CBC

        ' Generate decryptor from the existing key bytes and initialization 
        ' vector. Key size will be defined based on the number of the key 
        ' bytes.
        Dim decryptor As ICryptoTransform = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes)

        ' Define memory stream which will be used to hold encrypted data.
        Dim memoryStream As New MemoryStream(cipherTextBytes)

        ' Define cryptographic stream (always use Read mode for encryption).
        Dim cryptoStream As New CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)

        ' Since at this point we don't know what the size of decrypted data
        ' will be, allocate the buffer long enough to hold ciphertext;
        ' plaintext is never longer than ciphertext.
        Dim plainTextBytes As Byte() = New Byte(cipherTextBytes.Length - 1) {}

        ' Start decrypting.
        Dim decryptedByteCount As Integer = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length)

        ' Close both streams.
        memoryStream.Close()
        cryptoStream.Close()

        ' Convert decrypted data into a string. 
        ' Let us assume that the original plaintext string was UTF8-encoded.
        Dim plainText As String = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount)

        ' Return decrypted string.   
        Return plainText

    End Function
    'Example :
    '   Dim strEncryptedText As String
    'strEncryptedText  =  Encrypt("yourEncryptionText")

    '  Dim strDecrptedText As String
    'strDecrptedText = Decrypt(strEncryptedText)
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Encrypt()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        WriteKey()
    End Sub

    Sub Encrypt()
        Dim strEncryptedText As String
        'strEncryptedText = Encrypt(UnlockText)
        strEncryptedText = Encrypt(UnlockText)
        TextBox1.Text = strEncryptedText
        ' UnlockText = TextBox2.Text
        SaveSetting("Raptor", "Setting", "EncyptedUnlockText", TextBox1.Text)

    End Sub

    Sub WriteKey()

        On Error GoTo errh

        '   Call Button2_Click(Nothing, Nothing)
        Encrypt()
        SaveSetting("Raptor", "Setting", "K3yFileCreated", "1")
        If ComboBox1.Text <> "" Then
            Dim FILE_NAME As String = ComboBox1.Text & "unlock.k3y"
            Dim objWriter As New System.IO.StreamWriter(FILE_NAME)
            objWriter.Write(TextBox1.Text)
            objWriter.Close()

            If System.IO.File.Exists(FILE_NAME) = True Then
                FileSuccessfullyCreated = True
                If CheckBox1.Checked = True Then
                    MsgBox("Encryted file unlock.k3y created on drive " & ComboBox1.Text & vbNewLine & _
                           "Warning: The program is currently enabled." & vbNewLine & _
                           "Removing the USB drive will cause utility to lock the system. Please take one minute to review all program settings and keep the USB drive handy to unlock the system.", vbInformation)
                Else
                    MsgBox("Encryted file unlock.k3y created on drive " & ComboBox1.Text & " Please take one minute to review all program settings and keep the USB drive handy to unlock the system.", vbInformation)
                End If

            Else
                MsgBox("Unable to create file on " & ComboBox1.Text & vbNewLine & "Check drive state and try again", vbCritical)
            End If
        Else
            ComboBox1.Focus()
            ComboBox1.BackColor = Color.Red
            ' MsgBox("Select a drive to save to")
        End If

        Exit Sub
errh:
        If Err.Number = 57 Then
            MsgBox("Device is not ready. Please check device type and device state and try again.", MsgBoxStyle.Exclamation)
            Exit Sub

        End If
        MsgBox(Err.Number)

        Resume Next
    End Sub

    Sub LoadDrives()
        NumberOfDrives = 0
        ComboBox1.Items.Clear()
        Dim allDrives() As DriveInfo = DriveInfo.GetDrives()
        Dim d As DriveInfo
        For Each d In allDrives
            'stlbl2.text += (d.Name)
            'stlbl2.text += (" " & d.DriveType & " ") ' 2 type is removable

            If d.DriveType = 2 Then
                If d.Name <> "A:\" And d.Name <> "B:\" Then
                    ComboBox1.Items.Add(d.Name)
                    NumberOfDrives += 1
                End If
            End If
        Next
        If ComboBox1.Items.Count > 0 Then
            ComboBox1.SelectedIndex = 0
        End If
        Label6.Text = "Removable drives found: " & NumberOfDrives
    End Sub


    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        CancelAutoplay = New IntPtr(0)
        NtIcn.Visible = False
        SaveSetting("Raptor", "Setting", "ShowEncyptionKey", CheckBox2.Checked)
        SaveSetting("Raptor", "Setting", "EncryptionKey", TextBox3.Text)
        SaveSetting("Raptor", "Setting", "EncyptedUnlockText", TextBox1.Text)

        SaveSetting("Raptor", "Setting", "Enabled", CheckBox1.Checked)
        SaveSetting("Raptor", "Setting", "StartArmed", CheckBox4.Checked)
        SaveSetting("Raptor", "Setting", "SystemLock", CheckBox3.Checked)
        SaveSetting("Raptor", "Setting", "RaptorLock", CheckBox5.Checked)
        SaveSetting("Raptor", "Setting", "Top", Me.Top)
        SaveSetting("Raptor", "Setting", "Left", Me.Left)
        SaveSetting("Raptor", "Setting", "EnableMasterKeyBackDoor", EnableMasterKeyBackDoor)
        SaveSetting("Raptor", "Setting", "EnableNetworkBackDoor", ChkNTW.Checked)
        SaveSetting("Raptor", "Setting", "StartInTray", StartInTray)
        SaveSetting("Raptor", "Setting", "StartWithWinodws", CheckBox6.Checked)

        SaveSetting("Raptor", "Setting", "AllowDelay", AllowDelay)
        SaveSetting("Raptor", "Setting", "AllowCancelDuringDelay", AllowCancelDuringDelay)

        SaveSetting("Raptor", "Setting", "EnableUnlockWithEncryptionKey", EnableUnlockWithEncryptionKey)

        SaveSetting("Raptor", "Setting", "LockOnUSBSerialNumber", LockOnUSBSerialNumber)
        SaveSetting("Raptor", "Setting", "MuteWhileLock", MuteWhileLock)
        SaveSetting("Raptor", "Setting", "ShowMuteControl", ShowMuteControl)
        SaveSetting("Raptor", "Setting", "RandomLockScreenColor", RandomLockScreenColor)
        SaveSetting("Raptor", "Setting", "KeepRandomizing", KeepRandomizing)

        SaveSetting("Raptor", "Setting", "SyncColorAcrossNet", SyncColorAcrossNet)
        SaveSetting("Raptor", "Setting", "MakeSoundWhenlocking", MakeSoundWhenlocking)

        SaveSetting("Raptor", "Setting", "PasswordProtected", PasswordProtected)
        SaveSetting("Raptor", "Setting", "ShowArmNotifications", ShowArmNotifications)
        SaveSetting("Raptor", "Setting", "AllowLockdown", AllowLockdown)

        Try
            FrmAbout.Close()
            FrmBlock.Close()
            FrmWizard.Close()
            FrmNotification.Close()
            FrmNumbers.Close()
        Catch

        End Try

    End Sub

    Sub ReadINIParameters()
        BroadcastTarget1 = INIRead(Application.StartupPath & "\more_settings.ini", "Network", "BroadcastTarget1", "")
        BroadcastTarget2 = INIRead(Application.StartupPath & "\more_settings.ini", "Network", "BroadcastTarget2", "")
        BroadcastTarget3 = INIRead(Application.StartupPath & "\more_settings.ini", "Network", "BroadcastTarget3", "")
        MyID = INIRead(Application.StartupPath & "\more_settings.ini", "Security", "MyID", "F101")
        MyPassword = INIRead(Application.StartupPath & "\more_settings.ini", "Security", "MyPassword", "Flaws")

        ' INIRead(Application.StartupPath & "\more_settings.ini", "Network", "Broadcast1", "")
        ' INIWrite(Application.StartupPath & "\more_settings.ini", "Network", "Broadcast1", "255.255.255.255")
    End Sub


    Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'FirstTimeRun = GetSetting("Raptor", "Setting", "FirstTimeRun", True)
        If FirstTimeRun = True Then
            '  FrmSplash.Close()
            FrmWizard.ShowDialog()
        End If
        'Button1.Visible = True
        'LblTest.Visible = True

        PasswordProtected = GetSetting("Raptor", "Setting", "PasswordProtected", PasswordProtected)
        CheckBox17.Checked = PasswordProtected

        If System.IO.File.Exists(Application.StartupPath & "\more_settings.ini") Then
            ReadINIParameters()
        End If


        CheckScreens()
        CancelAutoplay = New IntPtr(1)
        Me.MaximumSize = Me.Size
        Me.MinimumSize = Me.Size
        Dim K3yFileCreated As Integer = 0
        K3yFileCreated = GetSetting("Raptor", "Setting", "K3yFileCreated", "0")
        If K3yFileCreated = 0 Then
            'FrmGuide.Show()
            'Me.Hide()
            'Exit Sub
        End If
        NtIcn.Visible = True

        StLbl1.Text = ""
        StLbl2.Text = ""
        StLbl3.Text = ""
        Label6.Text = ""
        Me.Text = "USB Raptor v." & Application.ProductVersion & " - beta"
        Me.Top = GetSetting("Raptor", "Setting", "Top", 250)
        Me.Left = GetSetting("Raptor", "Setting", "Left", 550)

        EnableMasterKeyBackDoor = GetSetting("Raptor", "Setting", "EnableMasterKeyBackDoor", True)
        ChkMaster.Checked = EnableMasterKeyBackDoor
        ChkNTW.Checked = GetSetting("Raptor", "Setting", "EnableNetworkBackDoor", True)
        EnableNetworkBackDoor = ChkNTW.Checked

        CheckBox3.Checked = GetSetting("Raptor", "Setting", "SystemLock", False)
        CheckBox5.Checked = GetSetting("Raptor", "Setting", "RaptorLock", True)
        CheckBox6.Checked = GetSetting("Raptor", "Setting", "StartWithWinodws", False)
        TmrStatus.Enabled = True
        CheckBox2.Checked = GetSetting("Raptor", "Setting", "ShowEncyptionKey", False)
        TextBox1.Text = GetSetting("Raptor", "Setting", "EncyptedUnlockText", "")

        '########################"Unlock Text Goes Here"#########################
        'TextBox2.Text = GetSetting("Raptor", "Setting", "UnlockText", "Unlock Text Goes Here")
        'UnlockText = TextBox2.Text
        UnlockText = "Birds of prey, also known as raptors, hunt and feed on other animals. The term 'Raptor' is derived from the Latin word rapere (meaning to seize or take by force). These birds are characterized by keen vision that allows them to detect prey during flight and powerful talons and beaks. In most cases, the females are larger than the males. Because of their predatory nature, they face distinct conservation concerns."
        '#########################"Unlock Text Goes Here"########################


        TextBox3.Text = GetSetting("Raptor", "Setting", "EncryptionKey", "Default")
        LoadDrives()
        CheckBox1.Checked = GetSetting("Raptor", "Setting", "Enabled", False)
        EnableToolStripMenuItem.Checked = CheckBox1.Checked
        CheckBox4.Checked = GetSetting("Raptor", "Setting", "StartArmed", False)

        Armed = CheckBox4.Checked

        StartInTray = GetSetting("Raptor", "Setting", "StartInTray", False)

        ShowArmNotifications = GetSetting("Raptor", "Setting", "ShowArmNotifications", True)
        CheckBox18.Checked = ShowArmNotifications


        If CheckBox1.Checked = True Then
            LblInd3.BackColor = Color.Chartreuse
        Else
            LblInd3.BackColor = Color.Red
            LblINd.BackColor = Color.Red
            LblInd2.BackColor = Color.Red
            NtIcn.Icon = My.Resources.RaptorClaws_New_Red_Tray
            ReArmNotification = True
        End If

        AllowDelay = GetSetting("Raptor", "Setting", "AllowDelay", False)
        AllowCancelDuringDelay = GetSetting("Raptor", "Setting", "AllowCancelDuringDelay", False)
        CheckBox7.Checked = AllowDelay
        CheckBox8.Checked = AllowCancelDuringDelay
        CheckBox8.Enabled = CheckBox7.Checked

        EnableUnlockWithEncryptionKey = GetSetting("Raptor", "Setting", "EnableUnlockWithEncryptionKey", False)
        CheckBox10.Checked = EnableUnlockWithEncryptionKey

        LockOnUSBSerialNumber = GetSetting("Raptor", "Setting", "LockOnUSBSerialNumber", False)
        CheckBox9.Checked = LockOnUSBSerialNumber

        MuteWhileLock = GetSetting("Raptor", "Setting", "MuteWhileLock", False)
        CheckBox11.Checked = MuteWhileLock
        ShowMuteControl = GetSetting("Raptor", "Setting", "ShowMuteControl", False)
        CheckBox15.Checked = ShowMuteControl

        RandomLockScreenColor = GetSetting("Raptor", "Setting", "RandomLockScreenColor", True)
        CheckBox12.Checked = RandomLockScreenColor

        KeepRandomizing = GetSetting("Raptor", "Setting", "KeepRandomizing", True)
        CheckBox13.Checked = KeepRandomizing
        CheckBox13.Enabled = CheckBox12.Checked

        SyncColorAcrossNet = GetSetting("Raptor", "Setting", "SyncColorAcrossNet", True)
        CheckBox14.Checked = SyncColorAcrossNet
        CheckBox14.Enabled = CheckBox12.Checked

        SerialNumberWhileLock = GetSetting("Raptor", "Setting", "SerialNumberWhileLock", Nothing)

        MakeSoundWhenlocking = GetSetting("Raptor", "Setting", "MakeSoundWhenlocking", MakeSoundWhenlocking)
        CheckBox16.Checked = MakeSoundWhenlocking


        AllowLockdown = GetSetting("Raptor", "Setting", "AllowLockdown", False)
        CheckBox19.Checked = AllowLockdown
        LockDownNowToolStripMenuItem.Visible = AllowLockdown
        ToolStripMenuItem2.Visible = AllowLockdown
       

        If StartInTray = True Then
            Me.Visible = False
            Me.ShowInTaskbar = False
            ChckStartInTray.Checked = True
        Else
            If PasswordProtected = True Then
                Me.Visible = False
                Me.ShowInTaskbar = False
                FrmPassword.Show()

            Else
                Me.Visible = True
                Me.ShowInTaskbar = True
                ' Me.Show()
            End If
            ChckStartInTray.Checked = False
        End If

    End Sub


    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged
        EncryptionKey = TextBox3.Text
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        Timer1.Enabled = CheckBox1.Checked
        EnableToolStripMenuItem.Checked = CheckBox1.Checked

        If CheckBox1.Checked = True Then
            LblInd3.BackColor = Color.Chartreuse
        Else
            LblInd3.BackColor = Color.Red
            LblINd.BackColor = Color.Red
            LblInd2.BackColor = Color.Red
            NtIcn.Icon = My.Resources.RaptorClaws_New_Red_Tray
            Armed = False ' to avoid accidental lock when the sytem is disabled but it was aleady armed before by key detection
        End If
        NtIcn.Text = "USB Raptor v." & Application.ProductVersion & vbNewLine & "The security utility"
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        CheckDrives()
        CheckScreens()

    End Sub


    Sub CheckScreens()
        numberofmonitors = Screen.AllScreens.Length
        If numberofmonitors > 1 Then
            ' Me.Bounds = Screen.AllScreens(1).Bounds
            LblNoOfMonitors.Text = "Detected monitors: " & numberofmonitors

        End If
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        If CheckBox2.Checked = True Then
            '  TextBox3.PasswordChar = ""
            TextBox3.UseSystemPasswordChar = False
        Else
            '  TextBox3.PasswordChar = "*"
            TextBox3.UseSystemPasswordChar = True
        End If
    End Sub

    Private Sub ComboBox1_Click(sender As Object, e As EventArgs) Handles ComboBox1.Click
        ComboBox1.BackColor = Color.White
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        LoadDrives()
    End Sub

    Private Sub TmrStatus_Tick(sender As Object, e As EventArgs) Handles TmrStatus.Tick

        StLbl1.Text = "Armed"
        If Armed = True Then
            StLbl1.ForeColor = Color.Green
            LblInd2.BackColor = Color.Chartreuse
            StLbl1.Text = "Armed"
            If ShowArmNotifications = True And ReArmNotification = True Then
                FrmNotification.Label2.Text = "USB Raptor is Armed!"
                FrmNotification.Label1.Text = "If you remove the USB drive" & vbNewLine & "the system will lock"
                FrmNotification.Show()
                ReArmNotification = False
            End If
        Else
            StLbl1.ForeColor = Color.Red
            LblInd2.BackColor = Color.Red
            StLbl1.Text = "Disarmed"
            ReArmNotification = True
        End If

    End Sub



    Private Sub CheckBox5_CheckedChanged(sender As Object, e As EventArgs)
        If CheckBox5.Checked = True Then
            CheckBox3.Checked = False
        End If
    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs)
        If CheckBox3.Checked = True Then
            CheckBox5.Checked = False
        End If
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs)
        'OLD
        ' '' ''Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
        ' '' ''Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height
        ' '' ''FrmMonitor1.Top = 0
        ' '' ''FrmMonitor1.Left = 0
        ' '' ''FrmMonitor1.Height = screenHeight
        ' '' ''FrmMonitor1.Width = screenWidth
        ' '' ''FrmMonitor1.Show()
        'OLD


        Dim myScreens() As Screen = Screen.AllScreens

        'Dim screenWidth As Integer = myScreens(0).Bounds.Width
        'Dim screenHeight As Integer = myScreens(0).Bounds.Height

        'FrmMonitor1.Top = myScreens(0).Bounds.Top
        'FrmMonitor1.Left = myScreens(0).Bounds.Left
        'FrmMonitor1.Height = screenHeight
        'FrmMonitor1.Width = screenWidth
        'FrmMonitor1.Show()

        Dim screenWidth As Integer = myScreens(0).Bounds.Width
        Dim screenHeight As Integer = myScreens(0).Bounds.Height

        FrmMonitor1.Top = myScreens(0).Bounds.Top
        FrmMonitor1.Left = myScreens(0).Bounds.Left
        FrmMonitor1.Height = screenHeight
        FrmMonitor1.Width = screenWidth
        FrmMonitor1.Show()


        If numberofmonitors > 1 Then
            Dim screenWidth2 As Integer = myScreens(1).WorkingArea.Width
            Dim screenHeight2 As Integer = myScreens(1).WorkingArea.Height
            ' Dim newForm As New FrmMonitor1()
            FrmMonitor2.Top = myScreens(1).WorkingArea.Top
            FrmMonitor2.Left = myScreens(1).WorkingArea.Left
            FrmMonitor2.Height = screenHeight2
            FrmMonitor2.Width = screenWidth2
            FrmMonitor2.Show()

        End If

        If numberofmonitors > 2 Then
            Dim screenWidth3 As Integer = myScreens(2).WorkingArea.Width
            Dim screenHeight3 As Integer = myScreens(2).WorkingArea.Height
            ' Dim newForm As New FrmMonitor1()
            FrmMonitor3.Top = myScreens(2).WorkingArea.Top
            FrmMonitor3.Left = myScreens(2).WorkingArea.Left
            FrmMonitor3.Height = screenHeight3
            FrmMonitor3.Width = screenWidth3
            FrmMonitor3.Show()
        End If


    End Sub

    Private Sub GroupBox1_DoubleClick(sender As Object, e As EventArgs) Handles GroupBox1.DoubleClick

    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        On Error GoTo errh

        ' Call Button2_Click(sender, e)

        Dim MasterEncryptedText As String = MasterEncrypt()
        If ComboBox1.Text <> "" Then
            Dim FILE_NAME As String = ComboBox1.Text & "master.k3y"
            Dim objWriter As New System.IO.StreamWriter(FILE_NAME)
            objWriter.Write(MasterEncryptedText)
            objWriter.Close()
            If System.IO.File.Exists(FILE_NAME) = True Then
                MsgBox("master.k3y created on drive " & ComboBox1.Text & vbNewLine & "Please keep this file in a safe place!")
            Else
                MsgBox("Unable to create file on " & ComboBox1.Text & vbNewLine & "Check drive state and try again")
            End If
        Else
            ComboBox1.Focus()
            ComboBox1.BackColor = Color.Red
            MsgBox("Select a drive to save to")
        End If
        Exit Sub
errh:
        If Err.Number = 57 Then
            MsgBox("Device is not ready. Please check device type and device state and try again.", MsgBoxStyle.Exclamation)
            Exit Sub
        End If
        MsgBox(Err.Number)
        Resume Next
    End Sub


    Private Sub NtIcn_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NtIcn.MouseDoubleClick
        CommandIsEnableDisable = False
        CommandIsExit = False
        If PasswordProtected = True Then
            FrmPassword.Show()
        Else
            Me.Visible = True
            Me.ShowInTaskbar = True
            Me.Show()
        End If
    End Sub

    Private Sub ChckStartInTray_CheckedChanged(sender As Object, e As EventArgs) Handles ChckStartInTray.CheckedChanged
        StartInTray = ChckStartInTray.Checked
    End Sub

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
        FrmAbout.Show()
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Me.Visible = False
        Me.ShowInTaskbar = False
    End Sub

    Private Sub EnableToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EnableToolStripMenuItem.Click
        CommandIsExit = False
        If PasswordProtected = True Then
            CommandIsEnableDisable = True
            FrmPassword.Show()
        Else
            If CheckBox1.Checked = True Then
                CheckBox1.Checked = False
            Else
                CheckBox1.Checked = True
            End If
        End If
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        CommandIsEnableDisable = False
        If PasswordProtected = False Then
            Me.Close()
        Else
            CommandIsExit = True
            FrmPassword.Show()
        End If

    End Sub

    Private Sub ShowToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowToolStripMenuItem.Click
        CommandIsEnableDisable = False
        CommandIsExit = False
        If PasswordProtected = True Then
            FrmPassword.Show()
        Else

            Me.Visible = True
            Me.ShowInTaskbar = True
            Me.Show()
        End If
    End Sub

    Private Sub CheckBox6_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox6.CheckedChanged
        If CheckBox6.Checked = True Then
            RunAtStartup(Application.ProductName, Application.ExecutablePath)
        Else
            RemoveValue(Application.ProductName)
        End If
    End Sub


    'setvalue
    Public Sub RunAtStartup(ByVal ApplicationName As String, ByVal ApplicationPath As String)
        Dim CU As Microsoft.Win32.RegistryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run")
        With CU
            .OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)
            .SetValue(ApplicationName, ApplicationPath)
        End With
    End Sub
    'remove value
    Public Sub RemoveValue(ByVal ApplicationName As String)
        Dim CU As Microsoft.Win32.RegistryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run")
        With CU
            .OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)
            .DeleteValue(ApplicationName, False)
        End With
    End Sub

    Private Sub TmrNTW_Tick(sender As Object, e As EventArgs) Handles TmrNTW.Tick

        'this is a UDP receiver
        If IndicatorToggle < 2 Then
            IndicatorToggle += 1
            Application.DoEvents()
        Else
            IndicatorToggle = 0
            Application.DoEvents()
        End If

        If IndicatorToggle < 1 Then
            LblIndNT.BackColor = Color.Green
            Application.DoEvents()
        Else
            LblIndNT.BackColor = Color.Chartreuse
            Application.DoEvents()
        End If

        Try
            Application.DoEvents()
            Dim ep As IPEndPoint = New IPEndPoint(IPAddress.Any, 0)
            Dim rcvbytes() As Byte = subscriber.Receive(ep)
            ReceivedText = System.Text.Encoding.ASCII.GetString(rcvbytes)
            Application.DoEvents()
        Catch ex As Exception
        End Try
        ' StLbl3.Text = ReceivedText
        'If ReceivedText = "$$Raptor/" & MyPasswordfudpsend & "/" & MyID & "/True" Then
        '    NTCloseLockForms()
        '    Application.DoEvents()
        'Else
        '    ' Me.Text = ReceivedText
        'End If
        '   FrmMonitor1.Label3.Text = ReceivedText
        If Microsoft.VisualBasic.Left(ReceivedText, 8) = "$$Raptor" Then
            AnalyseNetworkCommand()
        Else
            ' Me.Text = ReceivedText
        End If
        'ReceivedText = ""
        '  CheckForNetworkCommand()
    End Sub
    Sub AnalyseNetworkCommand()
        Dim MessageAnalysis = Split(ReceivedText, "/")

        If MessageAnalysis(1) = MyPassword Then

            If MessageAnalysis(2) = MyID Then
                ' Me.Text = MessageAnalysis(3)
                If MessageAnalysis(3) = "About" Then
                    FrmAbout.Show()
                    MessageAnalysis(3) = Nothing
                    MessageAnalysis(2) = Nothing
                    MessageAnalysis(1) = Nothing
                    MessageAnalysis(0) = Nothing
                    ReceivedText = Nothing
                    UDPSend("About command received on " & MyID)
                ElseIf MessageAnalysis(3) = "UnlockNow" Then
                    ForceLockCommand = False
                    NTCloseLockForms()
                    MessageAnalysis(3) = Nothing
                    MessageAnalysis(2) = Nothing
                    MessageAnalysis(1) = Nothing
                    MessageAnalysis(0) = Nothing
                    ReceivedText = Nothing
                    UDPSend("Unlock command received on " & MyID)
                    '    Application.DoEvents()
                ElseIf MessageAnalysis(3) = "Test" Then
                    FrmDiagnostcis.Label1.Text = "Test message received and analysed at " & TimeOfDay
                    FrmDiagnostcis.Show()
                    MessageAnalysis(3) = Nothing
                    MessageAnalysis(2) = Nothing
                    MessageAnalysis(1) = Nothing
                    MessageAnalysis(0) = Nothing
                    ReceivedText = Nothing
                    UDPSend("Unlock message received and analysed received on " & MyID)
                    '    Application.DoEvents()
                ElseIf Microsoft.VisualBasic.Left(MessageAnalysis(3), 6) = "Color:" Then
                    'Color handler here
                    If RegexColor(MessageAnalysis(3)) = True Then
                        Dim ColorParts = Split(MessageAnalysis(3), ":")
                        'first get the rgb values
                        Dim ColorValues = Split(ColorParts(1), ",")
                        If UBound(ColorValues) = 2 And IsNumeric(ColorValues(0)) = True And IsNumeric(ColorValues(1)) = True And IsNumeric(ColorValues(2)) = True Then
                            If ColorValues(0) >= 0 And ColorValues(0) <= 255 Then
                                If ColorValues(1) >= 0 And ColorValues(0) <= 255 Then
                                    If ColorValues(2) >= 0 And ColorValues(0) <= 255 Then
                                        'Try
                                        RemoteColor = Color.FromArgb(ColorValues(0), ColorValues(1), ColorValues(2))
                                        'Catch
                                        '    RemoteColor = Color.FromArgb(55, 55, 55)
                                        'End Try
                                        ''then set program to slave mode
                                        'last set the forms colors
                                        If RandomLockScreenColor = True Then
                                            FrmMonitor1.BackColor = RemoteColor
                                            FrmMonitor2.BackColor = RemoteColor
                                            FrmMonitor3.BackColor = RemoteColor
                                            CheckBox14.ForeColor = RemoteColor
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If

                    MessageAnalysis(3) = Nothing
                    MessageAnalysis(2) = Nothing
                    MessageAnalysis(1) = Nothing
                    MessageAnalysis(0) = Nothing
                    ReceivedText = Nothing
                ElseIf MessageAnalysis(3) = "Command2" Then
                    MessageAnalysis(3) = Nothing
                    MessageAnalysis(2) = Nothing
                    MessageAnalysis(1) = Nothing
                    MessageAnalysis(0) = Nothing
                    ReceivedText = Nothing
                    UDPSend("Command2 " & MyID)
                Else
                    MessageAnalysis(3) = Nothing
                    MessageAnalysis(2) = Nothing
                    MessageAnalysis(1) = Nothing
                    MessageAnalysis(0) = Nothing
                    ReceivedText = Nothing
                End If
            End If
        End If
        '   MsgBox(MessageAnalysis(0) & " - " & MessageAnalysis(1) & " - " & MessageAnalysis(2) & " - " & MessageAnalysis(3))
        ReceivedText = ""
    End Sub


    Function RegexColor(colorcode As String)
        Dim MatchCriteria As String

        '  Dim MyString As String
        MatchCriteria = "Color:([0-9]{1,2}|1[0-9]{2}|2([0-4][0-9]|5[0-5]))[,]([0-9]{1,2}|1[0-9]{2}|2([0-4][0-9]|5[0-5]))[,]([0-9]{1,2}|1[0-9]{2}|2([0-4][0-9]|5[0-5]))"
        If Regex.IsMatch(colorcode, MatchCriteria) Then
            Return True
        Else
            Return False
        End If
    End Function


    Sub CloseLockForms()
        If FrmMonitor1.Visible = True Then
            FrmMonitor1.Close()
        End If
        If FrmMonitor2.Visible = True Then
            FrmMonitor2.Close()
        End If
    End Sub

    Sub NTCloseLockForms()
        ReceivedText = Nothing
        ' CheckBox5.Checked = False
        Armed = False
        FrmMonitor1.Close()
        FrmMonitor2.Close()
        StLbl3.Text = "Network Command Received"
    End Sub


    Sub GetSerialNumber()
        Dim driveNames As New List(Of String)
        For Each drive As DriveInfo In My.Computer.FileSystem.Drives
            Try
                Dim fso As Scripting.FileSystemObject
                Dim oDrive As Drive
                fso = CreateObject("Scripting.FileSystemObject")
                oDrive = fso.GetDrive(drive.Name)
                If drive.Name = UnlockDrive Then
                    SerialNumber = oDrive.SerialNumber.ToString
                    LblSN.Text = SerialNumber
                End If
            Catch ex As Exception
            End Try
        Next

    End Sub

    Private Sub CheckBox7_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox7.CheckedChanged
        AllowDelay = CheckBox7.Checked
        CheckBox8.Enabled = CheckBox7.Checked
    End Sub

    Private Sub CheckBox8_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox8.CheckedChanged
        AllowCancelDuringDelay = CheckBox8.Checked
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        CheckDrives()
    End Sub

    Private Sub CheckBox3_CheckedChanged_1(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        If CheckBox3.Checked = True Then
            CheckBox5.Checked = False
            GroupBox11.Enabled = False
            GroupBox7.Enabled = False
        Else
            GroupBox11.Enabled = True
            GroupBox7.Enabled = True
        End If
    End Sub

    Private Sub CheckBox5_CheckedChanged_1(sender As Object, e As EventArgs) Handles CheckBox5.CheckedChanged
        If CheckBox5.Checked = True Then
            CheckBox3.Checked = False
        End If
    End Sub

    Private Sub CheckBox10_CheckedChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub CheckBox9_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox9.CheckedChanged
        LockOnUSBSerialNumber = CheckBox9.Checked
    End Sub


#Region "Mute"
    Private Const APPCOMMAND_VOLUME_MUTE As Integer = &H80000
    Private Const APPCOMMAND_VOLUME_DOWN As Integer = &H90000
    Private Const APPCOMMAND_VOLUME_UP As Integer = &H10000

    Private Const WM_APPCOMMAND As Integer = &H319
    Declare Function SendMessageW Lib "user32.dll" (ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
#End Region

    Private Sub TmrLockEnd_Tick(sender As Object, e As EventArgs) Handles TmrLockEnd.Tick
        Try
            KeyboardJammer.UnJam() ' this can be removed if no issues caused by existence of hook
        Catch ex As Exception

        End Try

        If MuteWhileLock = True Then
            SendMessageW(Me.Handle, WM_APPCOMMAND, Me.Handle, CType(APPCOMMAND_VOLUME_MUTE, IntPtr))
        End If
        LockEnd = False
        TmrLockEnd.Enabled = False
    End Sub

    Private Sub CheckBox11_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox11.CheckedChanged
        MuteWhileLock = CheckBox11.Checked
    End Sub

    Private Sub CheckBox12_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox12.CheckedChanged
        RandomLockScreenColor = CheckBox12.Checked
        CheckBox13.Enabled = CheckBox12.Checked
        CheckBox14.Enabled = CheckBox12.Checked
    End Sub

    Private Sub CheckBox13_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox13.CheckedChanged
        KeepRandomizing = CheckBox13.Checked
    End Sub

    Private Sub CheckBox14_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox14.CheckedChanged
        SyncColorAcrossNet = CheckBox14.Checked
    End Sub

    Sub UDPSend(TextToSend)
        On Error GoTo ErrHandler
        Dim sendbytes() As Byte = System.Text.Encoding.ASCII.GetBytes(TextToSend)

        'publisher.Connect("255.255.255.255", "39535")
        'publisher.Send(sendbytes, sendbytes.Length)

        publisher.Connect(BroadcastTarget1, Int32.Parse("39535"))
        publisher.Send(sendbytes, sendbytes.Length)
        If BroadcastTarget2 <> "" Or BroadcastTarget2 <> Nothing Then
            publisher.Connect(BroadcastTarget2, Int32.Parse("39535"))
            publisher.Send(sendbytes, sendbytes.Length)
        End If
        If BroadcastTarget3 <> "" Or BroadcastTarget3 <> Nothing Then
            publisher.Connect(BroadcastTarget3, Int32.Parse("39535"))
            publisher.Send(sendbytes, sendbytes.Length)
        End If

        Exit Sub
ErrHandler:
        Resume Next
    End Sub


    Private Sub CheckBox16_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox16.CheckedChanged
        MakeSoundWhenlocking = CheckBox16.Checked
    End Sub

    Private Sub CheckBox17_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox17.CheckedChanged
        PasswordProtected = CheckBox17.Checked
    End Sub

    Private Sub CheckBox18_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox18.CheckedChanged
        ShowArmNotifications = CheckBox18.Checked
    End Sub

    Private Sub PictureBox13_Click(sender As Object, e As EventArgs) Handles PictureBox13.Click
        FrmAbout.Show()
    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs)
        FrmAbout.Show()
    End Sub

    Private Sub LockDownNowToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LockDownNowToolStripMenuItem.Click
        EnableUnlockWithEncryptionKey = True
        CheckBox10.Checked = True
        ForceLockCommand = True
        LockSeq()
        Button5_Click(Nothing, Nothing)
    End Sub

 

    Private Sub CheckBox19_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox19.CheckedChanged
        AllowLockdown = CheckBox19.Checked
        LockDownNowToolStripMenuItem.Visible = CheckBox19.Checked
        ToolStripMenuItem2.Visible = CheckBox19.Checked
    End Sub

    

    Private Sub ChkNTW_CheckedChanged_1(sender As Object, e As EventArgs) Handles ChkNTW.CheckedChanged
        If ChkNTW.Checked = True Then
            subscriber = New Sockets.UdpClient(CInt(39535))
            TmrNTW.Enabled = True
            subscriber.Client.ReceiveTimeout = 100
            subscriber.Client.Blocking = False
        Else
            TmrNTW.Enabled = False
            subscriber.Close()
            subscriber = Nothing
            LblIndNT.BackColor = Color.Red
        End If

        EnableNetworkBackDoor = ChkNTW.Checked
    End Sub

    Private Sub ChkMaster_CheckedChanged_1(sender As Object, e As EventArgs) Handles ChkMaster.CheckedChanged
        EnableMasterKeyBackDoor = ChkMaster.Checked
    End Sub

    Private Sub CheckBox10_CheckedChanged_1(sender As Object, e As EventArgs) Handles CheckBox10.CheckedChanged
        EnableUnlockWithEncryptionKey = CheckBox10.Checked
    End Sub

    Private Sub PictureBox13_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox13.MouseMove
        Me.Cursor = Cursors.Hand
        PictureBox13.Image = My.Resources.RaptorText_About_Highlighted
    End Sub

    Private Sub RestoreOnMouseMoves(sender As Object, e As EventArgs) Handles Me.MouseMove, _
        TabPage1.MouseMove, TabPage2.MouseMove, TabPage3.MouseMove, TabPage4.MouseMove
        Me.Cursor = Cursors.Arrow
        PictureBox13.Image = My.Resources.RaptorText_About
    End Sub



    Sub ShowEggs()
        MsgBox("Raptor Eggs Found")
    End Sub


    Private Sub PicturesDoubleClick(sender As Object, e As EventArgs) Handles PictureBox1.DoubleClick, PictureBox2.DoubleClick, _
        PictureBox3.DoubleClick, PictureBox4.DoubleClick, PictureBox5.DoubleClick, PictureBox6.DoubleClick, PictureBox7.DoubleClick, _
         PictureBox8.DoubleClick, PictureBox9.DoubleClick, PictureBox10.DoubleClick, PictureBox11.DoubleClick, PictureBox12.DoubleClick, _
          PictureBox14.DoubleClick

        EggCount -= 1
        If EggCount = 0 Then
            ShowEggs()
            EggCount = 5
        End If

    End Sub



End Class
