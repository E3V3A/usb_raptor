

Public Class FrmMonitor1
    Dim OskProcess
    Dim AgitateTimes = 10
    Dim PrevTextboxLeft
    Dim PrevTextboxTop
    Dim ProhibitRelocate As Boolean = False
    Dim ColorCode As String

    Declare Function FindWindow Lib "user32" Alias "FindWindowA" (ByVal lpClassName As String, ByVal lpWindowName As String) As Integer
    Declare Function SetWindowPos Lib "user32" (ByVal hwnd As Integer, ByVal hWndInsertAfter As Integer, ByVal x As Integer, ByVal y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal wFlags As Integer) As Integer

    Public Const SWP_HIDEWINDOW = &H80
    Public Const SWP_SHOWWINDOW = &H40

    Dim RandomizeTime As Integer = 5
#Region "Mute"
    Private Const APPCOMMAND_VOLUME_MUTE As Integer = &H80000
    Private Const APPCOMMAND_VOLUME_DOWN As Integer = &H90000
    Private Const APPCOMMAND_VOLUME_UP As Integer = &H10000

    Private Const WM_APPCOMMAND As Integer = &H319
    Declare Function SendMessageW Lib "user32.dll" (ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
#End Region




    '#Region "KeyboardHook"
    '    Declare Function SetWindowsHookEx Lib "user32" Alias "SetWindowsHookExA" (ByVal idHook As Integer, ByVal lpfn As LowLevelKeyboardProcDelegate, ByVal hMod As IntPtr, ByVal dwThreadId As Integer) As IntPtr
    '    Declare Function UnhookWindowsHookEx Lib "user32" Alias "UnhookWindowsHookEx" (ByVal hHook As IntPtr) As Boolean
    '    Declare Function CallNextHookEx Lib "user32" Alias "CallNextHookEx" (ByVal hHook As IntPtr, ByVal nCode As Integer, ByVal wParam As Integer, ByRef lParam As KBDLLHOOKSTRUCT) As Integer
    '    Delegate Function LowLevelKeyboardProcDelegate(ByVal nCode As Integer, ByVal wParam As Integer, ByRef lParam As KBDLLHOOKSTRUCT) As Integer

    '    Const WH_KEYBOARD_LL As Integer = 13

    '    Structure KBDLLHOOKSTRUCT
    '        Dim vkCode As Integer
    '        Dim scanCode As Integer
    '        Dim flags As Integer
    '        Dim time As Integer
    '        Dim dwExtraInfo As Integer
    '    End Structure

    '    Dim intLLKey As IntPtr

    '    Private Function LowLevelKeyboardProc(ByVal nCode As Integer, ByVal wParam As Integer, ByRef lParam As KBDLLHOOKSTRUCT) As Integer
    '        Dim blnEat As Boolean = False

    '        Select Case wParam
    '            Case 256, 257, 260, 261
    '                'Alt+Tab, Alt+Esc, Ctrl+Esc, Windows Key    
    '                blnEat = ((lParam.vkCode = 9) AndAlso (lParam.flags = 32)) Or _
    '((lParam.vkCode = 27) AndAlso (lParam.flags = 32)) Or _
    '((lParam.vkCode = 27) AndAlso (lParam.flags = 0)) Or _
    '((lParam.vkCode = 91) AndAlso (lParam.flags = 1)) Or _
    '((lParam.vkCode = 92) AndAlso (lParam.flags = 1))
    '        End Select
    '        If blnEat = True Then
    '            Return 1
    '        Else
    '            Return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam)
    '        End If
    '    End Function


    '#End Region



    'Dim WithEvents K As New Keyboard
    'Structure contain information about low-level keyboard input event

    Private Declare Function GetAsyncKeyState Lib "user32" _
       (ByVal vKey As Long) As Integer



    Private Sub FrmMonitor1_DoubleClick(sender As Object, e As EventArgs) Handles Me.DoubleClick
        'TextBox1.Visible = True
        'TextBox1.BackColor = Color.Gray

        If EnableUnlockWithEncryptionKey = True Then
            Panel2.Top = ((Me.Height - Panel2.Height) / 2) + 100
            Panel2.Left = (Me.Width - Panel2.Width) / 2
            Panel2.Visible = True
            TmrPop.Enabled = False
            Panel1.Top = Panel2.Top - Panel1.Height
            Panel1.Left = Panel2.Left
            Me.BackColor = Color.FromArgb(50, 50, 50) ' Color.MidnightBlue
            For Each Label In Me.Controls
                Label.forecolor = Color.Gray
            Next
            PictureBox1.Image = My.Resources.RaptorClaws_New_Gray_Tray1
            PictureBox1.Height = 150
            ' PictureBox1.Width = 150
            PictureBox1.Top = 55
            ' Label3.Text = PictureBox1.Top
            FrmMonitor2.PictureBox1.Image = My.Resources.RaptorClaws_New_Gray_Tray1
            FrmMonitor2.PictureBox1.Height = 150
            '  FrmMonitor2.PictureBox1.Width = 150
            FrmMonitor2.PictureBox1.Top = 55

            FrmMonitor3.PictureBox1.Image = My.Resources.RaptorClaws_New_Gray_Tray1
            FrmMonitor3.PictureBox1.Height = 150
            ' FrmMonitor3.PictureBox1.Width = 150
            FrmMonitor3.PictureBox1.Top = 55
            TextBox1.Focus()

            FrmMonitor2.BackColor = Me.BackColor
            For Each Label In FrmMonitor2.Controls
                Label.forecolor = Color.Gray
            Next
            FrmMonitor3.BackColor = Me.BackColor
            For Each Label In FrmMonitor3.Controls
                Label.forecolor = Color.Gray
            Next
            RandomLockScreenColor = False

        Else
            RandomColor()
            Tmrpop_Tick(Nothing, Nothing)
            FrmMonitor2.BackColor = Me.BackColor
            FrmMonitor3.BackColor = Me.BackColor
        End If


    End Sub


    ' Public Declare Function apiBlockInput Lib "user32" Alias "BlockInput" (ByVal fBlock As Integer) As Integer



    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim time As DateTime = DateTime.Now
        '  Dim format As String = "MMM ddd d HH:mm yyyy"
        Dim format As String = "HH:mm:ss"
        LblClock.Text = time.ToString(format)
        Me.TopMost = True
        '  apiBlockInput(1)
        ' SendKeys.Send("{F5}")
    End Sub




    'Private Sub FrmMonitor1_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
    '    If e.KeyCode = Keys.Alt Then
    '        e.Handled = True
    '    End If
    'End Sub
    Private Sub RandomColor()
        Dim rand As New Random
        Dim Red As Integer
        Dim Green As Integer
        Dim Blue As Integer
        Red = rand.Next(0, 256)
        Green = rand.Next(0, 256)
        Blue = rand.Next(0, 256)
        ColorCode = (Red & "," & Green & "," & Blue).ToString


        Me.BackColor = Color.FromArgb(Red, Green, Blue)

        If SyncColorAcrossNet = True Then
            FrmMain.UDPSend("$$Raptor/" & MyPassword & "/" & MyID & "/Color:" & ColorCode)

        End If

    End Sub

    Private Sub FrmMonitor1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' intLLKey = SetWindowsHookEx(WH_KEYBOARD_LL, AddressOf LowLevelKeyboardProc, IntPtr.Zero, 0)


        Try
            FrmPassword.Close()
        Catch ex As Exception

        End Try


        FrmBlock.Show()
        'If ShowMuteControl = True Then
        '    LinkLabel1.Visible = True
        'Else
        '    LinkLabel1.Visible = False
        'End If
        Dim UnlockText As String
        If ForceLockCommand = False Then
            If LockOnUSBSerialNumber = False Then
                unlocktext = "Valid ways to unlock: Insert a USB key with valid unlock file"
            Else
                unlocktext = "Valid ways to unlock: Insert the USB key with serial number ******" & Microsoft.VisualBasic.Right(SerialNumberWhileLock, 4) & " and valid unlock file"
            End If
        Else

            UnlockText = "Valid ways to unlock: "
            Label3.Text = "Lockdown Active"
        End If


        If EnableUnlockWithEncryptionKey = True Then
            If ForceLockCommand = False Then
                UnlockText = UnlockText & " - Enter the password (double click to activate login screen)"
            Else
                UnlockText = UnlockText & " Enter the password (double click to activate login screen)"
            End If

        End If
            If EnableNetworkBackDoor = True Then
                UnlockText = UnlockText & " - Send a valid network unlock message"
            End If
            If EnableMasterKeyBackDoor = True Then
                UnlockText = UnlockText & " - Use the USB master key"
            End If

            LblUnlockOptions.Text = UnlockText


            If RandomLockScreenColor = True Then
                RandomColor()
            Else
                Me.BackColor = Color.FromArgb(130, 130, 130)
                FrmMonitor2.BackColor = Me.BackColor
                FrmMonitor3.BackColor = Me.BackColor


            End If

            Dim cm As New ContextMenu
            TextBox1.ContextMenu = cm

            Dim intReturn As Integer = FindWindow("Shell_traywnd", "")
            SetWindowPos(intReturn, 0, 0, 0, 0, 0, SWP_HIDEWINDOW)
            Try
                My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System\", "DisableTaskMgr", "1")
            Catch

            End Try

            Label2.Text = "Version: " & Application.ProductVersion
            ' Timer1.Enabled = True
            ' Call KillControlAltDelete(True)
            '   apiBlockInput(1) '''This command blocks user input!

            '''''''' Below new blockage
            Dim process As System.Diagnostics.Process = Nothing
            Dim psi As New ProcessStartInfo
            psi.UseShellExecute = True
            psi.FileName = "taskkill.exe"
            psi.CreateNoWindow = True

            psi.Arguments = "/F /IM explorer.exe"
            process = System.Diagnostics.Process.Start(psi)

            '''''''
            psi.FileName = "taskkill.exe"
            psi.CreateNoWindow = True

            psi.Arguments = "/F /IM dualmonitor.exe"
            process = System.Diagnostics.Process.Start(psi)


            TmrPop.Enabled = True
            Timer1.Enabled = True
            Tmrpop_Tick(Nothing, Nothing)
            LblClock.Top = 20
            LblClock.Left = (Me.Width / 2) - (LblClock.Width / 2)
            If MuteWhileLock = True Then
                SendMessageW(Me.Handle, WM_APPCOMMAND, Me.Handle, CType(APPCOMMAND_VOLUME_MUTE, IntPtr)) ' mute command
            End If

            If MakeSoundWhenlocking = True Then
                '  My.Computer.Audio.Play(My.Resources.Magic_Wand_Noise, AudioPlayMode.WaitToComplete)
                TmrSound.Enabled = True
            End If
    End Sub




    Private Sub FrmMonitor1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        '  UnhookWindowsHookEx(intLLKey)
        'SendMessageW(Me.Handle, WM_APPCOMMAND, Me.Handle, CType(APPCOMMAND_VOLUME_MUTE, IntPtr))
        'SendMessageW(Me.Handle, WM_APPCOMMAND, Me.Handle, CType(APPCOMMAND_VOLUME_DOWN, IntPtr)) ' unmute command
        'SendMessageW(Me.Handle, WM_APPCOMMAND, Me.Handle, CType(APPCOMMAND_VOLUME_UP, IntPtr)) ' unmute command
        'FrmBlock.Close()

        If OskProcess Is Nothing Then
            OskProcess = New Process()
            OskProcess.StartInfo.UseShellExecute = True
            OskProcess.StartInfo.CreateNoWindow = True
            Try
                OskProcess.StartInfo.FileName = "C:\Windows\explorer.exe"
            Catch
            End Try
            OskProcess.StartInfo.WorkingDirectory = Application.StartupPath
            OskProcess.Start()
            OskProcess = Nothing
        End If
        'If OskProcess Is Nothing Then
        '    OskProcess = New Process()
        '    OskProcess.StartInfo.UseShellExecute = True
        '    OskProcess.StartInfo.CreateNoWindow = True
        '    OskProcess.StartInfo.FileName = "C:\Program Files (x86)\Dual Monitor\DualMonitor.exe"
        '    OskProcess.StartInfo.WorkingDirectory = Application.StartupPath
        '    OskProcess.Start()

        'End If
        ' Shell("C:\Program Files (x86)\Dual Monitor\DualMonitor.exe")

        Dim intReturn As Integer = FindWindow("Shell_traywnd", "")
        SetWindowPos(intReturn, 0, 0, 0, 0, 0, SWP_SHOWWINDOW)
        '   Call KillControlAltDelete(False)
        'System.Threading.Thread.Sleep(3000)
        '   apiBlockInput(0) '''This command restores user input!
        Try
            My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System\", "DisableTaskMgr", "")

        Catch
        End Try
        LockEnd = True
        FrmMain.TmrLockEnd.Enabled = True
        FrmBlock.Close()


    End Sub





    Private Sub Tmrpop_Tick(sender As Object, e As EventArgs) Handles TmrPop.Tick
        ' Initialize the random-number generator.
        Randomize()
        ' Generate random value between 1 and 6. 
        ' apiBlockInput(0)
        Dim LimitVertical As Integer = Me.Height - Panel1.Height
        Dim LimitHorizontal As Integer = Me.Width - Panel1.Width

        Dim ValueVertical As Integer = CInt(Int((LimitVertical * Rnd()) + 1))
        Dim ValueHorizontal As Integer = CInt(Int((LimitHorizontal * Rnd()) + 1))

        Panel1.Top = ValueVertical
        Panel1.Left = ValueHorizontal
        If RandomLockScreenColor = True And KeepRandomizing = True Then
            If RandomizeTime > 1 Then
                RandomizeTime -= 1
            Else
                RandomizeTime = 5
                RandomColor()
                FrmMonitor2.BackColor = Me.BackColor
                FrmMonitor3.BackColor = Me.BackColor
            End If

        End If

    End Sub

    Protected Overrides Function ProcessDialogKey(ByVal keyData As System.Windows.Forms.Keys) As Boolean

        Select Case (keyData)

            '   Case Keys.LWin Or Keys.Tab
            '      Return True
            '    Case Keys.Delete
            '         Return True
            Case Keys.Alt Or Keys.F4
                Return True
        End Select
        Return MyBase.ProcessDialogKey(keyData)


    End Function



    'Protected Overrides Function ProcessCmdKey(ByRef msg As Message, ByVal keyData As Keys) As Boolean
    '    If keyData = (Keys.Alt Or Keys.Tab) Then
    '        Return True
    '    Else
    '        Return MyBase.ProcessCmdKey(msg, keyData)
    '    End If
    'End Function







    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If TextBox1.Text = EncryptionKey Then
            ForceLockCommand = False
            Armed = False
            Me.Close()
            If FrmMonitor2.Visible Then
                FrmMonitor2.Close()

            End If
            If FrmMonitor3.Visible Then
                FrmMonitor3.Close()

            End If


        Else
            If ProhibitRelocate = False Then
                PrevTextboxLeft = TextBox1.Left
                PrevTextboxTop = TextBox1.Top
            End If

            TextBox1.Enabled = False
            TmrAgitate.Enabled = True
            ProhibitRelocate = True
            '  Call Agitate()

            End If
    End Sub

    Private Sub TextBox1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox1.KeyPress
        If e.KeyChar = Chr(Keys.Enter) Then
            Button1_Click(Nothing, Nothing)
        End If
    End Sub

    Private Sub TextBox1_LostFocus(sender As Object, e As EventArgs) Handles TextBox1.LostFocus
        TextBox1.Focus()
    End Sub

    Dim objRandom As New System.Random(CType(System.DateTime.Now.Ticks Mod System.Int32.MaxValue, Integer))

    Public Function GetRandomNumber(Optional ByVal Low As Integer = 1, Optional ByVal High As Integer = 100) As Integer
        ' Returns a random number,
        ' between the optional Low and High parameters
        Return objRandom.Next(Low, High + 1)
    End Function
    Public Function IsEven(ByVal Number As Long) As Boolean
        IsEven = (Number Mod 2 = 0)
    End Function

    Private Sub TmrAgitate_Tick(sender As Object, e As EventArgs) Handles TmrAgitate.Tick

        If AgitateTimes > 0 Then
            AgitateTimes -= 1


            Dim intDiceRoll As Integer
            intDiceRoll = GetRandomNumber(2, 8)
            Dim intDiceRoll2 As Integer
            intDiceRoll2 = GetRandomNumber(1, 4)



            If IsEven(AgitateTimes) = True Then
                TextBox1.Left = PrevTextboxLeft - intDiceRoll
            Else
                TextBox1.Left = PrevTextboxLeft + intDiceRoll
            End If


            If IsEven(intDiceRoll2) = True Then
                TextBox1.Top = PrevTextboxTop - intDiceRoll2
            Else
                TextBox1.Top = PrevTextboxTop + intDiceRoll2
            End If

        Else
            AgitateTimes = 10
            TmrAgitate.Enabled = False
            ProhibitRelocate = False
            TextBox1.Left = PrevTextboxLeft
            TextBox1.Top = PrevTextboxTop
            TextBox1.Enabled = True


        End If


    End Sub


    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        SendMessageW(Me.Handle, WM_APPCOMMAND, Me.Handle, CType(APPCOMMAND_VOLUME_MUTE, IntPtr)) ' mute command
    End Sub

  

    Private Sub TmrSound_Tick(sender As Object, e As EventArgs) Handles TmrSound.Tick
        My.Computer.Audio.Play(My.Resources.Magic_Wand_Noise, AudioPlayMode.WaitToComplete)
        TmrSound.Enabled = False
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub
End Class
