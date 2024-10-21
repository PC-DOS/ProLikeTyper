Imports System.Reflection
Imports System.IO
Imports System.Text
Imports System.Windows.Threading
Public Class NetScanWindow
    'Timer object for "coding"
    Const TypingInterval As Double = 735
    Const CursorFalshingInterval As Double = 500
    Dim CoderTimer As New DispatcherTimer()
    Dim CursorTimer As New DispatcherTimer()

    'Global randowm generator
    Dim RandomGen As New Random

    'Code source
    Const InternalCodeSource As String = "CoderText.txt"
    Const ExternalCodeSource As String = "CoderText.txt"
    Dim CodeSourceList As New List(Of String)
    Dim CodeSourceLinePointer As Integer
    Dim CodeSourceColumnPointer As Integer

    'Display text of code
    Dim CodeText As String = ""
    Dim CodeTextDisplay As String = ""

    'Host info
    Const ResponseTimeMax As Double = 2.45
    Const ResponseTimeMin As Double = 0.245
    Dim HostIPPrefix As String
    Dim HostIPSuffix As Integer
    Dim HostIP As String
    Dim IsHostAlive As Boolean
    Dim IsHostVulnerable As Boolean

    'Console cursor
    Dim IsConsoleCursorVisible As Boolean = True
    Const ConsoleCursorFull As String = "▮"
    Const ConsoleCursorEmpty As String = "▯"

    'States
    Enum NetScanStates
        SelectingAddress = 0
        PrintingPingHeader = 1
        Ping1 = 2
        Ping2 = 3
        Ping3 = 4
        Ping4 = 5
        PrintingPingResult = 6
        ScaningVulnerability1 = 7
        PrintingVulnerability1Result = 8
        ScaningVulnerability2 = 9
        PrintingVulnerability2Result = 10
        ScaningVulnerability3 = 11
        PrintingVulnerability3Result = 12
        PrintingEndMessage = -1
    End Enum
    Dim StateMachine As NetScanStates

    Private Function GenerateRandomDouble(ValMin As Double, ValMax As Double) As Double
        Return RandomGen.NextDouble() * (ValMax - ValMin) + ValMin
    End Function

    Private Function GenerateRandomHexString(Length As Integer) As String
        Dim HexResult As String = ""
        For i As Integer = 1 To Length
            'Add number or letter
            If RandomGen.Next(0, 2) = 0 Then
                HexResult = HexResult & Chr(RandomGen.Next(48, 57))
            Else
                HexResult = HexResult & Chr(RandomGen.Next(97, 122))
            End If
        Next
        Return HexResult
    End Function

    Private Sub InitializeNewScan()
        'Initialize new scan
        Dim AddressPrefix As Integer = RandomGen.Next(1, 5)
        Select Case AddressPrefix
            Case 1
                HostIPPrefix = "192.0.2."
            Case 2
                HostIPPrefix = "198.51.100."
            Case 3
                HostIPPrefix = "203.0.113."
            Case 4
                HostIPPrefix = "233.252.0."
            Case Else
                HostIPPrefix = "203.0.113."
        End Select
        HostIPSuffix = 0
    End Sub

    Private Sub GenerateNextHost()
        'IP range: [1, 254]
        HostIPSuffix += 1
        If HostIPSuffix >= 255 Then
            InitializeNewScan()
            HostIPSuffix = 1
        End If
        HostIP = HostIPPrefix & HostIPSuffix.ToString()

        'Host availability
        If RandomGen.Next(0, 10) <= 2 Then
            IsHostAlive = True
        End If
        IsHostVulnerable = False
    End Sub

    Private Sub NetScanWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Initialize state machine
        StateMachine = NetScanStates.SelectingAddress

        'Initialize scan
        InitializeNewScan()
        GenerateNextHost()
        CodeText = ""

        'Initialize timers
        AddHandler CoderTimer.Tick, AddressOf CoderTimer_Tick
        CoderTimer.Interval = TimeSpan.FromMilliseconds(TypingInterval)
        CoderTimer.Start()
        AddHandler CursorTimer.Tick, AddressOf CursorTimer_Tick
        CursorTimer.Interval = TimeSpan.FromMilliseconds(CursorFalshingInterval)
        CursorTimer.Start()
    End Sub

    Private Sub CoderTimer_Tick()
        'State judging & updating
        Select Case StateMachine
            Case NetScanStates.SelectingAddress
                CodeText = CodeText & _
                           "Current target: " & HostIP & vbCrLf
                StateMachine = NetScanStates.PrintingPingHeader
            Case NetScanStates.PrintingPingHeader
                CodeText = CodeText & _
                           "Sending ICMP sequence to " & HostIP & " for host availability checking..." & vbCrLf
                StateMachine = NetScanStates.Ping1
            Case NetScanStates.Ping1
                If IsHostAlive Then
                    CodeText = CodeText & _
                               "Reply from " & HostIP & ": bytes=32 " & "time=" & GenerateRandomDouble(ResponseTimeMin, ResponseTimeMax).ToString() & "ms, TTL=128" & vbCrLf
                Else
                    CodeText = CodeText & _
                               "Response timed out." & vbCrLf
                End If
                StateMachine = NetScanStates.Ping2
            Case NetScanStates.Ping2
                If IsHostAlive Then
                    CodeText = CodeText & _
                               "Reply from " & HostIP & ": bytes=32 " & "time=" & GenerateRandomDouble(ResponseTimeMin, ResponseTimeMax).ToString() & "ms, TTL=128" & vbCrLf
                Else
                    CodeText = CodeText & _
                               "Response timed out." & vbCrLf
                End If
                StateMachine = NetScanStates.Ping3
            Case NetScanStates.Ping3
                If IsHostAlive Then
                    CodeText = CodeText & _
                               "Reply from " & HostIP & ": bytes=32 " & "time=" & GenerateRandomDouble(ResponseTimeMin, ResponseTimeMax).ToString() & "ms, TTL=128" & vbCrLf
                Else
                    CodeText = CodeText & _
                               "Response timed out." & vbCrLf
                End If
                StateMachine = NetScanStates.Ping4
            Case NetScanStates.Ping4
                If IsHostAlive Then
                    CodeText = CodeText & _
                               "Reply from " & HostIP & ": bytes=32 " & "time=" & GenerateRandomDouble(ResponseTimeMin, ResponseTimeMax).ToString() & "ms, TTL=128" & vbCrLf
                Else
                    CodeText = CodeText & _
                               "Response timed out." & vbCrLf
                End If
                StateMachine = NetScanStates.PrintingPingResult
            Case NetScanStates.PrintingPingResult
                If IsHostAlive Then
                    CodeText = CodeText & _
                               "4 packets sent, 4 receivced, 0% loss. Starting vulnerability scanning..." & vbCrLf
                Else
                    CodeText = CodeText & _
                               "4 packets sent, 0 receivced, 100% loss." & vbCrLf
                End If
                If IsHostAlive Then
                    StateMachine = NetScanStates.ScaningVulnerability1
                Else
                    StateMachine = NetScanStates.PrintingEndMessage
                End If
            Case NetScanStates.ScaningVulnerability1
                CodeText = CodeText & _
                           "Sending ACC_FAK_902 packet to " & HostIP & "for GSVA-22093 checking..." & vbCrLf
                StateMachine = NetScanStates.PrintingVulnerability1Result
            Case NetScanStates.PrintingVulnerability1Result
                If RandomGen.Next(0, 100) >= 95 Then
                    CodeText = CodeText & _
                               "Response with 0x" & GenerateRandomHexString(8).ToUpper() & " received. GSVA-22093 is available on host." & vbCrLf
                    IsHostVulnerable = True
                Else
                    CodeText = CodeText & _
                               "Error: connection timed out, or closed by host. GSVA-22093 detection failed." & vbCrLf
                End If
                StateMachine = NetScanStates.ScaningVulnerability2
            Case NetScanStates.ScaningVulnerability2
                CodeText = CodeText & _
                           "Sending RCL-REQ-I-3291 request to " & HostIP & ":" & RandomGen.Next(1024, 65536).ToString() & "for GSVA-23127 checking..." & vbCrLf
                StateMachine = NetScanStates.PrintingVulnerability2Result
            Case NetScanStates.PrintingVulnerability2Result
                If RandomGen.Next(0, 100) >= 95 Then
                    CodeText = CodeText & _
                               "Response with 0x" & GenerateRandomHexString(8).ToUpper() & "_" & GenerateRandomHexString(8).ToUpper() & " received. GSVA-23127 is available on host." & vbCrLf
                    IsHostVulnerable = True
                Else
                    CodeText = CodeText & _
                               "Response RCL_INT_ADMIN_DISABLED received. GSVA-23127 detection failed." & vbCrLf
                End If
                StateMachine = NetScanStates.ScaningVulnerability3
            Case NetScanStates.ScaningVulnerability3
                CodeText = CodeText & _
                           "Sending WSSP_QUERY_VERSION request to " & HostIP & "for GSVA-23542 checking..." & vbCrLf
                StateMachine = NetScanStates.PrintingVulnerability3Result
            Case NetScanStates.PrintingVulnerability3Result
                If RandomGen.Next(0, 100) >= 95 Then
                    CodeText = CodeText & _
                               "Response with 0x" & GenerateRandomHexString(8).ToUpper & " received. GSVA-23542 is available on host." & vbCrLf
                    IsHostVulnerable = True
                Else
                    CodeText = CodeText & _
                               "Error: connection timed out, or closed by host. GSVA-23542 detection failed." & vbCrLf
                End If
                StateMachine = NetScanStates.PrintingEndMessage
            Case NetScanStates.PrintingEndMessage
                If IsHostAlive Then
                    If IsHostVulnerable Then
                        CodeText = CodeText & _
                                   "Host " & HostIP & " is alive with 1 or more vulnerabilities detected." & vbCrLf
                    Else
                        CodeText = CodeText & _
                                   "Host " & HostIP & " is alive." & vbCrLf
                    End If
                Else
                    CodeText = CodeText & _
                               "Host " & HostIP & " is unreachable. Switching to next host." & vbCrLf
                End If
                CodeText = CodeText & vbCrLf
                GenerateNextHost()
                StateMachine = NetScanStates.SelectingAddress
        End Select

        'Concat cursor
        If IsConsoleCursorVisible Then
            CodeTextDisplay = CodeText & ConsoleCursorFull
        Else
            CodeTextDisplay = CodeText & ConsoleCursorEmpty
        End If

        'Update display
        lblCode.Text = CodeTextDisplay
        scrCodeContainer.ScrollToEnd()
    End Sub

    Private Sub CursorTimer_Tick()
        'Flip console's cursor status
        IsConsoleCursorVisible = Not IsConsoleCursorVisible
        If IsConsoleCursorVisible Then
            CodeTextDisplay = CodeText & ConsoleCursorFull
        Else
            CodeTextDisplay = CodeText & ConsoleCursorEmpty
        End If

        'Update display
        lblCode.Text = CodeTextDisplay
    End Sub
End Class
