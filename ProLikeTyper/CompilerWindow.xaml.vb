Imports System.Reflection
Imports System.IO
Imports System.Text
Imports System.Windows.Threading
Public Class CompilerWindow
    'Size of "file" and "database"
    Const SingleFileSizeByteMax As Double = 16 * 1024 * 1024
    Const SingleFileSizeByteMin As Double = 1 * 1024 * 1024
    Const FileCountMax As Integer = '24500
    Const FileCountMin As Integer = '2450
    Const DownloadSpeedByteMax As Double = 45 * 1024 * 1024
    Const DownloadSpeedByteMin As Double = 25 * 1024 * 1024
    Const DownloadSpeedGeneratingInterval As Double = 50
    Const DownloadSpeedCalaculatingInterval As Double = 1000

    'Data of current downloading file
    Dim CurrentFilePath As String
    Dim CurrentFileSizeTotalByte As Double
    Dim CurrentFileSizeDownloadedByte As Double

    'Data of current downloading database
    Dim CurrentDownloadingDatabaseLocation As String
    Dim CurrentDownloadingDatabaseFileCount As Integer
    Dim CurrentDownloadingFileIndex As Integer

    'Global randowm generator
    Dim RandomGen As New Random

    'Downloader simulation timer
    Dim FileDownloadTimer As New DispatcherTimer
    Dim DownloadSpeedCalaculatingTimer As New DispatcherTimer
    Dim CurrentDownloadSpeed As Double
    Dim DownloadSpeedDisplay As Double
    Dim IsDownloaderFirstRun As Boolean = True
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

    Private Sub InitializeRemoteDatabaseData()
        'Generate database address
        CurrentDownloadingDatabaseLocation = "//localhost:" & RandomGen.Next(1024, 65536).ToString() & "/svc_base/"

        'Generate database directory
        CurrentDownloadingDatabaseLocation = CurrentDownloadingDatabaseLocation & Guid.NewGuid().ToString() & "/"

        'Generate file count
        CurrentDownloadingDatabaseFileCount = RandomGen.Next(FileCountMin, FileCountMax + 1)
        CurrentDownloadingFileIndex = 1
    End Sub

    Private Sub GenerateNextFileInformation()
        'Judge "file info" by current step
        Select Case CurrentDownloadingFileIndex
            Case 1
                'Preprocessing
                CurrentFilePath = "Preprocessing source directory """ & CurrentDownloadingDatabaseLocation & """..."
                CurrentFileSizeTotalByte = SingleFileSizeByteMax * 5
                CurrentFileSizeDownloadedByte = 0
                Exit Sub
            Case CurrentDownloadingDatabaseFileCount - 1
                'Linking
                CurrentFilePath = "Assembling compiled files in """ & CurrentDownloadingDatabaseLocation & "/.comp/temp/""..."
                CurrentFileSizeTotalByte = SingleFileSizeByteMax * 4
                CurrentFileSizeDownloadedByte = 0
                Exit Sub
            Case CurrentDownloadingDatabaseFileCount
                'Linking
                CurrentFilePath = "Linking compiled files to """ & CurrentDownloadingDatabaseLocation & "/.comp/out/""..."
                CurrentFileSizeTotalByte = SingleFileSizeByteMax * 2
                CurrentFileSizeDownloadedByte = 0
                Exit Sub
        End Select

        'Generate path
        CurrentFilePath = ""
        Dim PathLevel As Integer = RandomGen.Next(1, 2)
        For i As Integer = 1 To PathLevel
            CurrentFilePath = CurrentFilePath & GenerateRandomHexString(RandomGen.Next(2, 10)) & "/"
        Next

        'Generate name
        CurrentFilePath = CurrentFilePath & GenerateRandomHexString(RandomGen.Next(5, 15))
        Dim SuffixID As Integer = RandomGen.Next(0, 10)
        Select Case SuffixID
            Case 0
                CurrentFilePath = CurrentFilePath & ".vb"
            Case 1
                CurrentFilePath = CurrentFilePath & ".cpp"
            Case 2
                CurrentFilePath = CurrentFilePath & ".c"
            Case 3
                CurrentFilePath = CurrentFilePath & ".h"
            Case 4
                CurrentFilePath = CurrentFilePath & ".py"
            Case 5
                CurrentFilePath = CurrentFilePath & ".v"
            Case 6
                CurrentFilePath = CurrentFilePath & ".sch"
            Case 7
                CurrentFilePath = CurrentFilePath & ".pas"
            Case 8
                CurrentFilePath = CurrentFilePath & ".ui"
            Case 9
                CurrentFilePath = CurrentFilePath & ".xaml"
            Case Else
                CurrentFilePath = CurrentFilePath & ".vb"
        End Select
        CurrentFilePath = "Compiling """ & CurrentDownloadingDatabaseLocation & CurrentFilePath & """..."

        'Generate size
        CurrentFileSizeTotalByte = GenerateRandomDouble(SingleFileSizeByteMin, SingleFileSizeByteMax)
        CurrentFileSizeDownloadedByte = 0
    End Sub

    Private Sub RefreshStatusIndicator()
        'Current file
        lblFileName.Text = CurrentFilePath
        prgDownloadCurrent.Minimum = 0
        prgDownloadCurrent.Maximum = CurrentFileSizeTotalByte
        prgDownloadCurrent.Value = CurrentFileSizeDownloadedByte
        lblFileSize.Text = ByteToMByte(CurrentFileSizeDownloadedByte).ToString("F2") & " MB / " & _
                         ByteToMByte(CurrentFileSizeTotalByte).ToString("F2") & " MB, " & _
                         ByteToMByte(DownloadSpeedDisplay).ToString("F2") * 8 & " Mbps"

        'Current Database
        prgDownloadTotal.Minimum = 0
        prgDownloadTotal.Maximum = CurrentDownloadingDatabaseFileCount
        prgDownloadTotal.Value = CurrentDownloadingFileIndex - 1
        lblFileCount.Text = (CurrentDownloadingFileIndex - 1).ToString() & " / " & CurrentDownloadingDatabaseFileCount.ToString & " Steps | 0 Errors | " & _
                            DownloadSpeedDisplay.ToString() & " Warnings"
    End Sub

    Private Sub ConpilerWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Generate data
        InitializeRemoteDatabaseData()
        GenerateNextFileInformation()
        CurrentDownloadSpeed = 0
        DownloadSpeedDisplay = 0
        RefreshStatusIndicator()

        'Initialize timers
        AddHandler FileDownloadTimer.Tick, AddressOf FileDownloadTimer_Tick
        FileDownloadTimer.Interval = TimeSpan.FromMilliseconds(DownloadSpeedGeneratingInterval)
        FileDownloadTimer.Start()
        AddHandler DownloadSpeedCalaculatingTimer.Tick, AddressOf DownloadSpeedCalaculatingTimer_Tick
        DownloadSpeedCalaculatingTimer.Interval = TimeSpan.FromMilliseconds(DownloadSpeedCalaculatingInterval)
        DownloadSpeedCalaculatingTimer.Start()
    End Sub

    Private Sub FileDownloadTimer_Tick()
        'Generate one-shot download speed
        'Generating interval is scaled by  (DownloadSpeedCalaculatingInterval / DownloadSpeedGeneratingInterval) from 1000 milliseconds
        Dim DownloadSpeedOnce As Double
        DownloadSpeedOnce = GenerateRandomDouble(DownloadSpeedByteMin, DownloadSpeedByteMax)
        DownloadSpeedOnce = DownloadSpeedOnce / (DownloadSpeedCalaculatingInterval / DownloadSpeedGeneratingInterval)
        'Avoid exceeding remaining file size
        DownloadSpeedOnce = Math.Min(DownloadSpeedOnce, CurrentFileSizeTotalByte - CurrentFileSizeDownloadedByte)

        'Simulate downloading
        CurrentFileSizeDownloadedByte += DownloadSpeedOnce
        CurrentDownloadSpeed += DownloadSpeedOnce

        'Check if we need to proceed to to next file
        If CurrentFileSizeDownloadedByte >= CurrentFileSizeTotalByte Then
            CurrentDownloadingFileIndex += 1
            GenerateNextFileInformation()
            If CurrentDownloadingFileIndex > CurrentDownloadingDatabaseFileCount Then
                InitializeRemoteDatabaseData()
                GenerateNextFileInformation()
            End If
        End If

        'Update indicators
        RefreshStatusIndicator()
    End Sub

    Private Sub DownloadSpeedCalaculatingTimer_Tick()
        'Generate "warning" count
        If RandomGen.Next(0, 1000) > 735 Then
            DownloadSpeedDisplay += 1
        End If
    End Sub
End Class
