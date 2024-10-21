Imports System.Reflection
Imports System.IO
Imports System.Text
Imports System.Windows.Threading
Public Class CompilerWindow
    'Size of "file" and "database"
    Const SingleStepSizeByteMax As Double = 16 * 1024 * 1024
    Const SingleStepSizeByteMin As Double = 1 * 1024 * 1024
    Const StepCountMax As Integer = 24500
    Const StepCountMin As Integer = 2450
    Const ProcessingSpeedByteMax As Double = 45 * 1024 * 1024
    Const ProcessingSpeedByteMin As Double = 25 * 1024 * 1024
    Const ProcessingSpeedGeneratingInterval As Double = 50
    Const ProcessingSpeedCalaculatingInterval As Double = 1000

    'Data of current compiling step
    Dim CurrentStepName As String
    Dim CurrentStepSizeTotalByte As Double
    Dim CurrentStepSizeProcessedByte As Double

    'Data of current codebase
    Dim CurrentCodebaseLocation As String
    Dim CurrentCodebaseStepCount As Integer
    Dim CurrentStepIndex As Integer

    'Global randowm generator
    Dim RandomGen As New Random

    'Compiler simulation timer
    Dim ProcessingTimer As New DispatcherTimer
    Dim ProcessingSpeedCalaculatingTimer As New DispatcherTimer
    Dim CurrentProcessingSpeed As Double
    Dim ProcessingSpeedDisplay As Double

    Private Function GenerateRandomDouble(ValMin As Double, ValMax As Double) As Double
        Return RandomGen.NextDouble() * (ValMax - ValMin) + ValMin
    End Function

    Private Function ByteToMByte(SizeByte As Double) As Double
        Return SizeByte / 1024 / 1024
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

    Private Sub InitializeCodebaseData()
        'Generate codebase address
        CurrentCodebaseLocation = "//localhost:" & RandomGen.Next(1024, 65536).ToString() & "/svc_base/"

        'Generate codebase directory
        CurrentCodebaseLocation = CurrentCodebaseLocation & Guid.NewGuid().ToString() & "/"

        'Generate step count
        CurrentCodebaseStepCount = RandomGen.Next(StepCountMin, StepCountMax + 1)
        CurrentStepIndex = 1
    End Sub

    Private Sub GenerateNextStepInformation()
        'Judge "step info" by current step
        Select Case CurrentStepIndex
            Case 1
                'Preprocessing
                CurrentStepName = "Preprocessing source directory """ & CurrentCodebaseLocation & """..."
                CurrentStepSizeTotalByte = SingleStepSizeByteMax * 5
                CurrentStepSizeProcessedByte = 0
                Exit Sub
            Case CurrentCodebaseStepCount - 1
                'Assembling
                CurrentStepName = "Assembling compiled files in """ & CurrentCodebaseLocation & "/.comp/temp/""..."
                CurrentStepSizeTotalByte = SingleStepSizeByteMax * 4
                CurrentStepSizeProcessedByte = 0
                Exit Sub
            Case CurrentCodebaseStepCount
                'Linking
                CurrentStepName = "Linking compiled files to """ & CurrentCodebaseLocation & "/.comp/out/""..."
                CurrentStepSizeTotalByte = SingleStepSizeByteMax * 2
                CurrentStepSizeProcessedByte = 0
                Exit Sub
        End Select

        'Generate path
        CurrentStepName = ""
        Dim PathLevel As Integer = RandomGen.Next(1, 2)
        For i As Integer = 1 To PathLevel
            CurrentStepName = CurrentStepName & GenerateRandomHexString(RandomGen.Next(2, 10)) & "/"
        Next

        'Generate name
        CurrentStepName = CurrentStepName & GenerateRandomHexString(RandomGen.Next(5, 15))
        Dim SuffixID As Integer = RandomGen.Next(0, 10)
        Select Case SuffixID
            Case 0
                CurrentStepName = CurrentStepName & ".vb"
            Case 1
                CurrentStepName = CurrentStepName & ".cpp"
            Case 2
                CurrentStepName = CurrentStepName & ".c"
            Case 3
                CurrentStepName = CurrentStepName & ".h"
            Case 4
                CurrentStepName = CurrentStepName & ".py"
            Case 5
                CurrentStepName = CurrentStepName & ".v"
            Case 6
                CurrentStepName = CurrentStepName & ".sch"
            Case 7
                CurrentStepName = CurrentStepName & ".pas"
            Case 8
                CurrentStepName = CurrentStepName & ".ui"
            Case 9
                CurrentStepName = CurrentStepName & ".xaml"
            Case Else
                CurrentStepName = CurrentStepName & ".vb"
        End Select
        CurrentStepName = "Compiling """ & CurrentCodebaseLocation & CurrentStepName & """..."

        'Generate size
        CurrentStepSizeTotalByte = GenerateRandomDouble(SingleStepSizeByteMin, SingleStepSizeByteMax)
        CurrentStepSizeProcessedByte = 0
    End Sub

    Private Sub RefreshStatusIndicator()
        'Current step
        lblStepName.Text = CurrentStepName
        prgCurrent.Minimum = 0
        prgCurrent.Maximum = CurrentStepSizeTotalByte
        prgCurrent.Value = CurrentStepSizeProcessedByte
        lblStepSize.Text = ByteToMByte(CurrentStepSizeProcessedByte).ToString("F2") & " MB / " & _
                           ByteToMByte(CurrentStepSizeTotalByte).ToString("F2") & " MB, " & _
                           ByteToMByte(ProcessingSpeedDisplay).ToString("F2") * 8 & " Mbps"

        'Current codebase
        prgTotalProgress.Minimum = 0
        prgTotalProgress.Maximum = CurrentCodebaseStepCount
        prgTotalProgress.Value = CurrentStepIndex - 1
        lblTotalProgress.Text = (CurrentStepIndex - 1).ToString() & " / " & CurrentCodebaseStepCount.ToString() & " Steps | 0 Errors | " & _
                            ProcessingSpeedDisplay.ToString() & " Warnings"
    End Sub

    Private Sub CompilerWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Generate data
        InitializeCodebaseData()
        GenerateNextStepInformation()
        CurrentProcessingSpeed = 0
        ProcessingSpeedDisplay = 0
        RefreshStatusIndicator()

        'Initialize timers
        AddHandler ProcessingTimer.Tick, AddressOf ProcessingTimer_Tick
        ProcessingTimer.Interval = TimeSpan.FromMilliseconds(ProcessingSpeedGeneratingInterval)
        ProcessingTimer.Start()
        AddHandler ProcessingSpeedCalaculatingTimer.Tick, AddressOf ProcessingSpeedCalaculatingTimer_Tick
        ProcessingSpeedCalaculatingTimer.Interval = TimeSpan.FromMilliseconds(ProcessingSpeedCalaculatingInterval)
        ProcessingSpeedCalaculatingTimer.Start()
    End Sub

    Private Sub ProcessingTimer_Tick()
        'Generate one-shot processing speed
        'Generating interval is scaled by  (ProcessingSpeedCalaculatingInterval / ProcessingSpeedGeneratingInterval) from 1000 milliseconds
        Dim ProcessingSpeedOnce As Double
        ProcessingSpeedOnce = GenerateRandomDouble(ProcessingSpeedByteMin, ProcessingSpeedByteMax)
        ProcessingSpeedOnce = ProcessingSpeedOnce / (ProcessingSpeedCalaculatingInterval / ProcessingSpeedGeneratingInterval)
        'Avoid exceeding remaining file size
        ProcessingSpeedOnce = Math.Min(ProcessingSpeedOnce, CurrentStepSizeTotalByte - CurrentStepSizeProcessedByte)

        'Simulate processing
        CurrentStepSizeProcessedByte += ProcessingSpeedOnce
        CurrentProcessingSpeed += ProcessingSpeedOnce

        'Check if we need to proceed to to next step
        If CurrentStepSizeProcessedByte >= CurrentStepSizeTotalByte Then
            CurrentStepIndex += 1
            GenerateNextStepInformation()
            If CurrentStepIndex > CurrentCodebaseStepCount Then
                InitializeCodebaseData()
                GenerateNextStepInformation()
            End If
        End If

        'Update indicators
        RefreshStatusIndicator()
    End Sub

    Private Sub ProcessingSpeedCalaculatingTimer_Tick()
        'Generate "warning" count
        If RandomGen.Next(0, 1000) > 735 Then
            ProcessingSpeedDisplay += 1
        End If
    End Sub
End Class
