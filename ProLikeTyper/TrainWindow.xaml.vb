Imports System.Reflection
Imports System.IO
Imports System.Text
Imports System.Windows.Threading
Public Class TrainWindow
    'Size of "file" and "database"
    Const SingleIterationSizeByteMax As Double = 245 * 1024 * 1024
    Const SingleIterationSizeByteMin As Double = 125 * 1024 * 1024
    Const IterationCountMax As Integer = 24500
    Const IterationCountMin As Integer = 2450
    Const EpochCountMax As Integer = 245
    Const EpochCountMin As Integer = 25
    Const TrainingSpeedByteMax As Double = 45 * 1024 * 1024
    Const TrainingSpeedByteMin As Double = 25 * 1024 * 1024
    Const TrainingSpeedGeneratingInterval As Double = 50
    Const TrainingSpeedCalaculatingInterval As Double = 1000

    'Data of current iteration
    Dim CurrentIterationName As String
    Dim CurrentIterationTotalByte As Double
    Dim CurrentIterationProcessedByte As Double

    'Data of current epoch
    Dim CurrentEpochLocation As String
    Dim CurrentEpochIterationCount As Integer
    Dim CurrentIterationIndex As Integer

    'Data of current train
    Dim CurrentTrainEpochCount As Integer
    Dim CurrentEpochIndex As Integer

    'Global randowm generator
    Dim RandomGen As New Random

    'Training simulation timer
    Dim TrainingTimer As New DispatcherTimer
    Dim TrainingSpeedCalaculatingTimer As New DispatcherTimer
    Dim CurrentTrainingSpeed As Double
    Dim TrainingSpeedDisplay As Double
    Dim IsTrainFirstRun As Boolean = True

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

    Private Sub InitializeTrainData()
        'Generate epoch count
        CurrentTrainEpochCount = RandomGen.Next(EpochCountMin, EpochCountMax + 1)
        CurrentEpochIndex = 1

        'Generate iteration count per epoch
        CurrentEpochIterationCount = RandomGen.Next(IterationCountMin, IterationCountMax + 1)
    End Sub

    Private Sub InitializeEpochData()
        'Initialize step count
        CurrentIterationIndex = 1
    End Sub

    Private Sub GenerateNextIterationInformation()
        'Generate iteration name
        CurrentIterationName = "Running iteration " & CurrentIterationIndex.ToString() & "..."

        'Generate size
        CurrentIterationTotalByte = GenerateRandomDouble(SingleIterationSizeByteMin, SingleIterationSizeByteMax)
        CurrentIterationProcessedByte = 0
    End Sub

    Private Sub RefreshStatusIndicator()
        'Current iteration
        lblIterationName.Text = CurrentIterationName
        prgCurrent.Minimum = 0
        prgCurrent.Maximum = CurrentIterationTotalByte
        prgCurrent.Value = CurrentIterationProcessedByte
        lblIterationSize.Text = ByteToMByte(CurrentIterationProcessedByte).ToString("F2") & " MB / " & _
                           ByteToMByte(CurrentIterationTotalByte).ToString("F2") & " MB, " & _
                           ByteToMByte(TrainingSpeedDisplay).ToString("F2") * 8 & " Mbps"

        'Current epoch
        prgEpochProgress.Minimum = 0
        prgEpochProgress.Maximum = CurrentEpochIterationCount
        prgEpochProgress.Value = CurrentIterationIndex - 1
        lblEpochProgress.Text = (CurrentIterationIndex - 1).ToString() & " / " & CurrentEpochIterationCount.ToString() & " Iterations | " & _
                                (TrainingSpeedDisplay / CurrentIterationTotalByte).ToString("F2") & " it/s"

        'Current train
        prgTotalEpoch.Minimum = 0
        prgTotalEpoch.Maximum = CurrentTrainEpochCount
        prgTotalEpoch.Value = CurrentEpochIndex - 1
        lblTotalCount.Text = (CurrentEpochIndex - 1).ToString() & " / " & CurrentTrainEpochCount.ToString() & " Epoches"
    End Sub

    Private Sub TrainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Generate data
        InitializeTrainData()
        InitializeEpochData()
        GenerateNextIterationInformation()
        CurrentTrainingSpeed = 0
        TrainingSpeedDisplay = 0
        RefreshStatusIndicator()

        'Initialize timers
        AddHandler TrainingTimer.Tick, AddressOf TrainingTimer_Tick
        TrainingTimer.Interval = TimeSpan.FromMilliseconds(TrainingSpeedGeneratingInterval)
        TrainingTimer.Start()
        AddHandler TrainingSpeedCalaculatingTimer.Tick, AddressOf TrainingSpeedCalaculatingTimer_Tick
        TrainingSpeedCalaculatingTimer.Interval = TimeSpan.FromMilliseconds(TrainingSpeedCalaculatingInterval)
        TrainingSpeedCalaculatingTimer.Start()
    End Sub

    Private Sub TrainingTimer_Tick()
        'Generate one-shot training speed
        'Generating interval is scaled by  (TrainingSpeedCalaculatingInterval / TrainingSpeedGeneratingInterval) from 1000 milliseconds
        Dim TrainingSpeedOnce As Double
        TrainingSpeedOnce = GenerateRandomDouble(TrainingSpeedByteMin, TrainingSpeedByteMax)
        TrainingSpeedOnce = TrainingSpeedOnce / (TrainingSpeedCalaculatingInterval / TrainingSpeedGeneratingInterval)
        'Avoid exceeding remaining file size
        TrainingSpeedOnce = Math.Min(TrainingSpeedOnce, CurrentIterationTotalByte - CurrentIterationProcessedByte)

        'Simulate training
        CurrentIterationProcessedByte += TrainingSpeedOnce
        CurrentTrainingSpeed += TrainingSpeedOnce

        'Check if we need to proceed to to next iteration
        If CurrentIterationProcessedByte >= CurrentIterationTotalByte Then
            CurrentIterationIndex += 1
            GenerateNextIterationInformation()
            If CurrentIterationIndex > CurrentEpochIterationCount Then
                CurrentEpochIndex += 1
                InitializeEpochData()
                GenerateNextIterationInformation()
                If CurrentEpochIndex > CurrentTrainEpochCount Then
                    InitializeTrainData()
                    InitializeEpochData()
                    GenerateNextIterationInformation()
                End If
            End If
        End If

        'Allow download speed to be refreshed quickly
        'Otherwise, it will keep 0 Mbps in first DownloadSpeedCalaculatingTimer interval
        If IsTrainFirstRun Then
            TrainingSpeedDisplay = CurrentTrainingSpeed * (TrainingSpeedCalaculatingInterval / TrainingSpeedGeneratingInterval)
            IsTrainFirstRun = False
        End If

        'Update indicators
        RefreshStatusIndicator()
    End Sub

    Private Sub TrainingSpeedCalaculatingTimer_Tick()
        TrainingSpeedDisplay = CurrentTrainingSpeed
        CurrentTrainingSpeed = 0
    End Sub
End Class
