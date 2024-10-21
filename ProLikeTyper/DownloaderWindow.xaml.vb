Imports System.Reflection
Imports System.IO
Imports System.Text
Imports System.Windows.Threading
Public Class DownloaderWindow
    'Size of "file" and "database"
    Const SingleFileSizeByteMax As Double = 64 * 1024 * 1024
    Const SingleFileSizeByteMin As Double = 5 * 1024 * 1024
    Const FileCountMax As Integer = 2450000
    Const FileCountMin As Integer = 24500
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
    Dim FileDownloadTimer As New DispatcherTimer(DispatcherPriority.Render)
    Dim DownloadSpeedCalaculatingTimer As New DispatcherTimer(DispatcherPriority.Render)
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
                HexResult = HexResult & Chr(RandomGen.Next(97, 103))
            End If
        Next
        Return HexResult
    End Function

    Private Sub InitializeRemoteDatabaseData()
        'Generate database address
        Dim DatabaseAddressPrefix As Integer = RandomGen.Next(1, 5)
        Select Case DatabaseAddressPrefix
            Case 1
                CurrentDownloadingDatabaseLocation = "//192.0.2." & RandomGen.Next(1, 254).ToString() & "/"
            Case 2
                CurrentDownloadingDatabaseLocation = "//198.51.100." & RandomGen.Next(1, 254).ToString() & "/"
            Case 3
                CurrentDownloadingDatabaseLocation = "//203.0.113." & RandomGen.Next(1, 254).ToString() & "/"
            Case 4
                CurrentDownloadingDatabaseLocation = "//233.252.0." & RandomGen.Next(1, 254).ToString() & "/"
            Case Else
                CurrentDownloadingDatabaseLocation = "//203.0.113." & RandomGen.Next(1, 254).ToString() & "/"
        End Select

        'Generate database directory
        CurrentDownloadingDatabaseLocation = CurrentDownloadingDatabaseLocation & Guid.NewGuid().ToString() & "/"

        'Generate file count
        CurrentDownloadingDatabaseFileCount = RandomGen.Next(FileCountMin, FileCountMax + 1)
        CurrentDownloadingFileIndex = 1
    End Sub

    Private Sub GenerateNextFileInformation()
        'Generate path
        CurrentFilePath = ""
        Dim PathLevel As Integer = RandomGen.Next(1, 4)
        For i As Integer = 1 To PathLevel
            CurrentFilePath = CurrentFilePath & GenerateRandomHexString(RandomGen.Next(5, 15)) & "/"
        Next

        'Generate name
        CurrentFilePath = CurrentFilePath & GenerateRandomHexString(RandomGen.Next(10, 25))

        'Generate size
        CurrentFileSizeTotalByte = GenerateRandomDouble(SingleFileSizeByteMin, SingleFileSizeByteMax)
        CurrentFileSizeDownloadedByte = 0
    End Sub

    Private Sub RefreshStatusIndicator()
        'Current file
        lblFileName.Text = CurrentDownloadingDatabaseLocation & CurrentFilePath
        prgDownloadCurrent.Minimum = 0
        prgDownloadCurrent.Maximum = CurrentFileSizeTotalByte
        prgDownloadCurrent.Value = CurrentFileSizeDownloadedByte
        lblFileSize.Text = ByteToMByte(CurrentFileSizeDownloadedByte).ToString("F2") & " MB / " & _
                         ByteToMByte(CurrentFileSizeTotalByte).ToString("F2") & " MB, " & _
                         ByteToMByte(DownloadSpeedDisplay).ToString("F2") * 8 & " Mbps"

        'Current database
        prgDownloadTotal.Minimum = 0
        prgDownloadTotal.Maximum = CurrentDownloadingDatabaseFileCount
        prgDownloadTotal.Value = CurrentDownloadingFileIndex - 1
        lblFileCount.Text = (CurrentDownloadingFileIndex - 1).ToString() & " / " & CurrentDownloadingDatabaseFileCount.ToString() & " Files"
    End Sub

    Private Sub DownloaderWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
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

        'Allow download speed to be refreshed quickly
        'Otherwise, it will keep 0 Mbps in first DownloadSpeedCalaculatingTimer interval
        If IsDownloaderFirstRun Then
            DownloadSpeedDisplay = CurrentDownloadSpeed * (DownloadSpeedCalaculatingInterval / DownloadSpeedGeneratingInterval)
            IsDownloaderFirstRun = False
        End If

        'Update indicators
        RefreshStatusIndicator()
    End Sub

    Private Sub DownloadSpeedCalaculatingTimer_Tick()
        DownloadSpeedDisplay = CurrentDownloadSpeed
        CurrentDownloadSpeed = 0
    End Sub

    Private Sub DownloaderWindow_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        'Change height of progress bars manually
        If Me.ActualHeight <= 300 Then
            prgDownloadCurrent.Height = 25
            prgDownloadTotal.Height = 25
        Else
            prgDownloadCurrent.Height = 40
            prgDownloadTotal.Height = 40
        End If
    End Sub
End Class
