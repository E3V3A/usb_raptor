'teken from http://www.daniweb.com/software-development/vbnet/threads/359374/disable-alttab-win-key-ctrl-esc-key
'You may have to go to Project / Properties / Debug and uncheck Enable the Visual Studio Hosting process for it to work while running in Visual Studio.




Imports System.Runtime.InteropServices
Imports System.Reflection

Public Class KeyboardJammer
    Private Delegate Function HookCallback(ByVal nCode As Integer, ByVal wParam As Integer, ByVal lParam As IntPtr) As Integer
    Private Shared HookDelegate As HookCallback
    Private Shared HookId As Integer
    Private Const Wh_Keyboard_LL As Integer = 13
    Private Const Vk_Tab As Integer = 9
    Private Const Vk_Escape As Integer = 27
    Private Const vk_Del As Integer = 46
    Private Const Vk_F4 As Integer = 115
    Private Const VK_LWinKey As Integer = 91
    Private Const VK_RWinKey As Integer = 92
    Private Shared Function KeyBoardHookProc(ByVal nCode As Integer, ByVal wParam As Integer, ByVal lParam As IntPtr) As Integer
        'All keyboard events will be sent here.
        'Don't process just pass along.
        If nCode < 0 Then
            Return CallNextHookEx(HookId, nCode, wParam, lParam)
        End If
        'Extract the keyboard structure from the lparam
        'This will contain the virtual key and any flags.
        'This is using the my.computer.keyboard to get the
        'flags instead
        Dim KeyboardSruct As KBDLLHOOKSTRUCT = Marshal.PtrToStructure(lParam, GetType(KBDLLHOOKSTRUCT))
        '     MsgBox(KeyboardSruct.vkCode.ToString, MsgBoxStyle.OkOnly)
        If KeyboardSruct.vkCode = Vk_Tab And My.Computer.Keyboard.AltKeyDown Then
            'Alt Tab
            Return 1
        ElseIf KeyboardSruct.vkCode = Vk_Escape And My.Computer.Keyboard.CtrlKeyDown Then
            'Control Escape
            Return 1
        ElseIf KeyboardSruct.vkCode = VK_LWinKey Or KeyboardSruct.vkCode = VK_RWinKey Then
            'Left Windows Key or Rigth Windows Key
            Return 1
        ElseIf KeyboardSruct.vkCode = vk_Del And My.Computer.Keyboard.CtrlKeyDown And My.Computer.Keyboard.AltKeyDown Then
            'Ctrl Alt Del
            Return 1
        ElseIf KeyboardSruct.vkCode = Vk_F4 And My.Computer.Keyboard.AltKeyDown Then
            'Alt F4
            Return 1
        End If
        'Send the message along 
        Return CallNextHookEx(HookId, nCode, wParam, lParam)
    End Function
    Public Shared Sub Jam()
        'Add the low level keyboard hook
        If HookId = 0 Then
            HookDelegate = AddressOf KeyBoardHookProc
            HookId = SetWindowsHookEx(Wh_Keyboard_LL, HookDelegate, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly.GetModules()(0)), 0)
            If HookId = 0 Then
                'error
         
            End If
        End If
    End Sub
    Public Shared Sub UnJam()
        'Remove the hook
        UnhookWindowsHookEx(HookId)
        HookId = 0
    End Sub
    <DllImport("user32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)> _
    Private Shared Function CallNextHookEx( _
       ByVal idHook As Integer, _
       ByVal nCode As Integer, _
       ByVal wParam As IntPtr, _
       ByVal lParam As IntPtr) As Integer
    End Function
    <DllImport("user32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)> _
    Private Shared Function SetWindowsHookEx( _
           ByVal idHook As Integer, _
           ByVal HookProc As HookCallback, _
           ByVal hInstance As IntPtr, _
           ByVal wParam As Integer) As Integer
    End Function
    <DllImport("user32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)> _
    Private Shared Function UnhookWindowsHookEx(ByVal idHook As Integer) As Integer
    End Function
    Private Structure KBDLLHOOKSTRUCT
        Public vkCode As Integer
        Public scanCode As Integer
        Public flags As Integer
        Public time As Integer
        Public dwExtraInfo As IntPtr
    End Structure
End Class