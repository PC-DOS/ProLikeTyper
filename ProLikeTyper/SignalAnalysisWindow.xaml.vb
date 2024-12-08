Imports System.Reflection
Imports System.IO
Imports System.Text
Imports System.Windows.Threading
Imports MathNet.Numerics.IntegralTransforms
Imports OxyPlot
Imports OxyPlot.Wpf
Public Class SignalAnalysisWindow
    'Data collecting options
    Const SignalDataWidth As Integer = 12
    Const SignalDataMin As Integer = -2 ^ (SignalDataWidth - 1)
    Const SignalDataMax As Integer = 2 ^ (SignalDataWidth - 1) - 1
    Const SignalDataPointCount As Integer = 256
    Const SignalNewPointCountPerCollecting As Integer = 32
    Const SpectrogramLineCount As Integer = 500
    Const DataPlottingTargetFrameRate As Integer = 25
    Const DataCollectingInterval As Integer = 1000 / DataPlottingTargetFrameRate
    Const SignalFFTMax As Double = SignalDataMax
    Const SignalFFTMin As Double = 0
    Const SignalFFTBufferSize As Integer = SignalDataPointCount + 2
    Const SignalSamplingRate As Double = DataPlottingTargetFrameRate * SignalNewPointCountPerCollecting

    'Signal source
    Const InternalSignalSource As String = "SigData.txt"
    Const ExternalSignalSource As String = "SigData.txt"
    Dim SignalSource As New List(Of Integer)
    Dim SignalSourceLinePointer As Integer

    'Data buffer and timer
    'Data collecting timer
    Dim DataCollectingTimer As New DispatcherTimer(DispatcherPriority.Render)
    'Data plotting related
    'Raw signal
    Dim SignalRawPlotter As New LineSeries
    Dim SignalRawDataSeries As OxyPlot.Series.LineSeries
    Dim SignalRawXAxis As Axis
    Dim SignalRawYAxis As Axis
    'FFT
    Dim SignalFFTBuffer(SignalFFTBufferSize) As Double
    Dim SignalFFTWindow(SignalFFTBufferSize) As Double
    Dim SignalFFTPlotter As New LineSeries
    Dim SignalFFTDataSeries As OxyPlot.Series.LineSeries
    Dim SignalFFTXAxis As Axis
    Dim SignalFFTYAxis As Axis
    'Spectrogram
    Dim SpectrogramBuffer(SpectrogramLineCount - 1, SignalDataPointCount / 2 - 1) As Double
    Dim SpectrogramPlotter As New HeatMapSeries
    Dim SpectrogramDataSeries As OxyPlot.Series.HeatMapSeries
    Dim SpectrogramXAxis As Axis
    Dim SpectrogramYAxis As Axis
    Dim SpectrogramZAxis As LinearColorAxis
    Dim CurrentSpectrogramColumn As Integer

    'Global randowm generator
    Dim RandomGen As New Random

    Private Function GenerateRandomDouble(ValMin As Double, ValMax As Double) As Double
        Return RandomGen.NextDouble() * (ValMax - ValMin) + ValMin
    End Function

    Private Function OxyPaletteToGradientStops(Palette As OxyPalette) As GradientStopCollection
        Dim PaletteGradient As New GradientStopCollection
        Dim ColorCount As Integer = Palette.Colors.Count
        Dim StopInterval As Double = 1 / (ColorCount - 1)

        'Special cases
        If ColorCount = 0 Then
            Return PaletteGradient
        ElseIf ColorCount = 1 Then
            PaletteGradient.Add(New GradientStop(Palette.Colors(0).ToColor(), 0))
            PaletteGradient.Add(New GradientStop(Palette.Colors(0).ToColor(), 1))
            Return PaletteGradient
        End If

        For i = 0 To ColorCount - 1
            PaletteGradient.Add(New GradientStop(Palette.Colors(i).ToColor(), i * StopInterval))
        Next
        Return PaletteGradient
    End Function

    Private Sub SignalAnalysisWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Try loading signal from external signal file
        Dim ExternalSignalFilePath As String = AppDomain.CurrentDomain.SetupInformation.ApplicationBase & "\" & ExternalSignalSource
        If File.Exists(ExternalSignalFilePath) Then
            Try
                Dim ExternalSignalFileStream As StreamReader = File.OpenText(ExternalSignalFilePath)
                Dim SignalString As String = ExternalSignalFileStream.ReadToEnd()
                Dim SignalPointString() As String = SignalString.Split(",")
                For Each SigPoint As String In SignalPointString
                    Try
                        SignalSource.Add(CInt(SigPoint))
                    Catch ex As Exception
                        SignalSource.Add(0)
                    End Try
                Next
                ExternalSignalFileStream.Close()
            Catch ex As Exception
                SignalSource.Clear()
            End Try
        End If

        'If no external signal file available, or signal file is empty, fall back to internal signal source
        If SignalSource.Count = 0 Then
            Dim InternalSignalFilePath As String = MethodBase.GetCurrentMethod().DeclaringType.Namespace & "." & InternalSignalSource
            Dim InternalSignalFileRawStream As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(InternalSignalFilePath)
            Dim InternalSignalFileStream As New StreamReader(InternalSignalFileRawStream)
            Dim SignalString As String = InternalSignalFileStream.ReadToEnd()
            Dim SignalPointString() As String = SignalString.Split(",")
            For Each SigPoint As String In SignalPointString
                SignalSource.Add(CInt(SigPoint))
            Next
        End If

        'Initialize signal pointers
        SignalSourceLinePointer = Int(GenerateRandomDouble(0, SignalSource.Count - 1))

        'Initialize timer
        DataCollectingTimer.Interval = TimeSpan.FromMilliseconds(DataCollectingInterval)
        AddHandler DataCollectingTimer.Tick, AddressOf DataCollectingTimer_Tick

        'Raw signal
        'Plotting area
        SignalRawPlotter.Color = Color.FromRgb(0, 0, 255)
        SignalRawPlotter.StrokeThickness = 1
        'Data series
        SignalRawDataSeries = SignalRawPlotter.CreateModel()
        For i As Integer = 1 To SignalDataPointCount
            SignalRawDataSeries.Points.Add(New DataPoint(i - 1, 0))
        Next
        'X-Axis
        SignalRawXAxis = New LinearAxis
        SignalRawXAxis.IsAxisVisible = True
        SignalRawXAxis.Layer = Axes.AxisLayer.AboveSeries
        SignalRawXAxis.AxislineStyle = LineStyle.Solid
        SignalRawXAxis.AxislineThickness = 1
        SignalRawXAxis.AxislineColor = Color.FromRgb(0, 0, 0)
        SignalRawXAxis.MajorGridlineStyle = LineStyle.Dash
        SignalRawXAxis.MajorGridlineThickness = 1
        SignalRawXAxis.MajorGridlineColor = Color.FromRgb(125, 125, 125)
        SignalRawXAxis.MajorStep = SignalDataPointCount / 4
        SignalRawXAxis.TextColor = Color.FromRgb(0, 0, 0)
        'SignalRawXAxis.Font = "Consolas"
        SignalRawXAxis.LabelFormatter = Function(Val As Double) ""
        SignalRawXAxis.Position = Axes.AxisPosition.Bottom
        SignalRawXAxis.Maximum = SignalDataPointCount
        SignalRawXAxis.Minimum = 0
        SignalRawXAxis.IsZoomEnabled = False
        SignalRawXAxis.IsPanEnabled = False
        'Y-Axis
        SignalRawYAxis = New LinearAxis
        SignalRawYAxis.IsAxisVisible = True
        SignalRawYAxis.Layer = Axes.AxisLayer.AboveSeries
        SignalRawYAxis.AxislineStyle = LineStyle.Solid
        SignalRawYAxis.AxislineThickness = 1
        SignalRawYAxis.AxislineColor = Color.FromRgb(0, 0, 0)
        SignalRawYAxis.MajorGridlineStyle = LineStyle.Dash
        SignalRawYAxis.MajorGridlineThickness = 1
        SignalRawYAxis.MajorGridlineColor = Color.FromRgb(125, 125, 125)
        SignalRawYAxis.MajorStep = (SignalDataMax - SignalDataMin + 1) / 4
        SignalRawYAxis.TextColor = Color.FromRgb(0, 0, 0)
        'SignalRawYAxis.Font = "Consolas"
        SignalRawYAxis.LabelFormatter = Function(Val As Double) ""
        SignalRawYAxis.Position = Axes.AxisPosition.Left
        SignalRawYAxis.Maximum = SignalDataMax + 2
        SignalRawYAxis.Minimum = SignalDataMin - 2
        SignalRawYAxis.IsZoomEnabled = False
        SignalRawYAxis.IsPanEnabled = False
        'Layout updating
        pltSignalRaw.IsLegendVisible = False
        pltSignalRaw.TitlePadding = 0
        pltSignalRaw.Axes.Add(SignalRawXAxis)
        pltSignalRaw.Axes.Add(SignalRawYAxis)
        pltSignalRaw.Series.Add(SignalRawPlotter)

        'FFT
        'Plotting area
        SignalFFTPlotter.Color = Color.FromRgb(0, 0, 255)
        SignalFFTPlotter.StrokeThickness = 1
        'Data series
        SignalFFTDataSeries = SignalFFTPlotter.CreateModel()
        For i As Integer = 1 To SignalDataPointCount
            SignalFFTBuffer(i) = SignalFFTMin
            SignalFFTDataSeries.Points.Add(New DataPoint(i - 1, SignalFFTMin))
        Next
        SignalFFTWindow = MathNet.Numerics.Window.Hamming(SignalFFTBufferSize)
        'X-Axis
        SignalFFTXAxis = New LinearAxis
        SignalFFTXAxis.IsAxisVisible = True
        SignalFFTXAxis.Layer = Axes.AxisLayer.AboveSeries
        SignalFFTXAxis.AxislineStyle = LineStyle.Solid
        SignalFFTXAxis.AxislineThickness = 1
        SignalFFTXAxis.AxislineColor = Color.FromRgb(0, 0, 0)
        SignalFFTXAxis.MajorGridlineStyle = LineStyle.Dash
        SignalFFTXAxis.MajorGridlineThickness = 1
        SignalFFTXAxis.MajorGridlineColor = Color.FromRgb(125, 125, 125)
        SignalFFTXAxis.MajorStep = SignalDataPointCount / 2 / 4
        SignalFFTXAxis.TextColor = Color.FromRgb(0, 0, 0)
        'SignalFFTXAxis.Font = "Consolas"
        SignalFFTXAxis.LabelFormatter = Function(Val As Double) ""
        SignalFFTXAxis.Position = Axes.AxisPosition.Bottom
        SignalFFTXAxis.Maximum = SignalDataPointCount / 2
        SignalFFTXAxis.Minimum = 0
        SignalFFTXAxis.IsZoomEnabled = False
        SignalFFTXAxis.IsPanEnabled = False
        'Y-Axis
        SignalFFTYAxis = New LinearAxis
        SignalFFTYAxis.IsAxisVisible = True
        SignalFFTYAxis.Layer = Axes.AxisLayer.AboveSeries
        SignalFFTYAxis.AxislineStyle = LineStyle.Solid
        SignalFFTYAxis.AxislineThickness = 1
        SignalFFTYAxis.AxislineColor = Color.FromRgb(0, 0, 0)
        SignalFFTYAxis.MajorGridlineStyle = LineStyle.Dash
        SignalFFTYAxis.MajorGridlineThickness = 1
        SignalFFTYAxis.MajorGridlineColor = Color.FromRgb(125, 125, 125)
        SignalFFTYAxis.MajorStep = SignalFFTMax / 4
        SignalFFTYAxis.TextColor = Color.FromRgb(0, 0, 0)
        'SignalFFTYAxis.Font = "Consolas"
        SignalFFTYAxis.LabelFormatter = Function(Val As Double) ""
        SignalFFTYAxis.Position = Axes.AxisPosition.Left
        SignalFFTYAxis.Maximum = SignalFFTMax + 2
        SignalFFTYAxis.Minimum = SignalFFTMin
        SignalFFTYAxis.IsZoomEnabled = False
        SignalFFTYAxis.IsPanEnabled = False
        'Layout updating
        pltSignalFFT.IsLegendVisible = False
        pltSignalFFT.TitlePadding = 0
        pltSignalFFT.Axes.Add(SignalFFTXAxis)
        pltSignalFFT.Axes.Add(SignalFFTYAxis)
        pltSignalFFT.Series.Add(SignalFFTPlotter)

        'Spectrogram
        CurrentSpectrogramColumn = 0
        'Plotting area
        'SpectrogramPlotter.Color = Color.FromRgb(0, 0, 255)
        SpectrogramPlotter.X0 = 0
        SpectrogramPlotter.X1 = SpectrogramLineCount
        SpectrogramPlotter.Y0 = 0
        SpectrogramPlotter.Y1 = SignalDataPointCount / 2
        'Data series
        SpectrogramDataSeries = SpectrogramPlotter.CreateModel()
        For col As Integer = 0 To SpectrogramLineCount - 1
            For row As Integer = 0 To SignalDataPointCount / 2 - 1
                SpectrogramBuffer(col, row) = SignalFFTMin - 1
            Next
        Next
        SpectrogramDataSeries.Interpolate = True
        SpectrogramDataSeries.RenderMethod = OxyPlot.Series.HeatMapRenderMethod.Bitmap
        SpectrogramPlotter.Data = SpectrogramBuffer
        'X-Axis
        SpectrogramXAxis = New LinearAxis
        SpectrogramXAxis.IsAxisVisible = True
        SpectrogramXAxis.Layer = Axes.AxisLayer.AboveSeries
        SpectrogramXAxis.AxislineStyle = LineStyle.Solid
        SpectrogramXAxis.AxislineThickness = 1
        SpectrogramXAxis.AxislineColor = Color.FromRgb(0, 0, 0)
        SpectrogramXAxis.MajorGridlineStyle = LineStyle.Dash
        SpectrogramXAxis.MajorGridlineThickness = 1
        SpectrogramXAxis.MajorGridlineColor = Color.FromArgb(0, 125, 125, 125)
        SpectrogramXAxis.MajorStep = SpectrogramLineCount / 4
        SpectrogramXAxis.TextColor = Color.FromRgb(0, 0, 0)
        'SpectrogramXAxis.Font = "Consolas"
        SpectrogramXAxis.LabelFormatter = Function(Val As Double) ""
        SpectrogramXAxis.Position = Axes.AxisPosition.Bottom
        SpectrogramXAxis.Maximum = SpectrogramLineCount
        SpectrogramXAxis.Minimum = 0
        SpectrogramXAxis.IsZoomEnabled = False
        SpectrogramXAxis.IsPanEnabled = False
        'Y-Axis
        SpectrogramYAxis = New LinearAxis
        SpectrogramYAxis.IsAxisVisible = True
        SpectrogramYAxis.Layer = Axes.AxisLayer.AboveSeries
        SpectrogramYAxis.AxislineStyle = LineStyle.Solid
        SpectrogramYAxis.AxislineThickness = 1
        SpectrogramYAxis.AxislineColor = Color.FromRgb(0, 0, 0)
        SpectrogramYAxis.MajorGridlineStyle = LineStyle.Dash
        SpectrogramYAxis.MajorGridlineThickness = 1
        SpectrogramYAxis.MajorGridlineColor = Color.FromArgb(0, 125, 125, 125)
        SpectrogramYAxis.MajorStep = SignalDataPointCount / 2 / 4
        SpectrogramYAxis.TextColor = Color.FromRgb(0, 0, 0)
        'SpectrogramYAxis.Font = "Consolas"
        SpectrogramYAxis.LabelFormatter = Function(Val As Double) ""
        SpectrogramYAxis.Position = Axes.AxisPosition.Left
        SpectrogramYAxis.Maximum = SignalDataPointCount / 2
        SpectrogramYAxis.Minimum = 0
        SpectrogramYAxis.IsZoomEnabled = False
        SpectrogramYAxis.IsPanEnabled = False
        'Z-Axis (Color Axis)
        SpectrogramZAxis = New LinearColorAxis
        SpectrogramZAxis.IsAxisVisible = True
        SpectrogramZAxis.TickStyle = Axes.TickStyle.None
        SpectrogramZAxis.HighColor = Colors.White
        SpectrogramZAxis.LowColor = Colors.Black
        SpectrogramZAxis.GradientStops = OxyPaletteToGradientStops(OxyPalettes.Jet(10))
        SpectrogramZAxis.PaletteSize = SignalFFTMax - SignalFFTMin + 1
        SpectrogramZAxis.Position = Axes.AxisPosition.Right
        SpectrogramZAxis.Maximum = SignalFFTMax
        SpectrogramZAxis.Minimum = SignalFFTMin
        SpectrogramZAxis.AxisDistance = 10
        SpectrogramZAxis.AxisTickToLabelDistance = 10
        'Layout updating
        pltSpectrogram.IsLegendVisible = False
        pltSpectrogram.TitlePadding = 0
        pltSpectrogram.Axes.Add(SpectrogramXAxis)
        pltSpectrogram.Axes.Add(SpectrogramYAxis)
        pltSpectrogram.Axes.Add(SpectrogramZAxis)
        pltSpectrogram.Series.Add(SpectrogramPlotter)

        'Start timers
        DataCollectingTimer.Start()
    End Sub

    Private Sub DataCollectingTimer_Tick()
        'Update raw signal data and prepare data for FFT
        For i = 0 To SignalDataPointCount - 1 - SignalNewPointCountPerCollecting
            Dim PreviousData As DataPoint
            PreviousData = SignalRawDataSeries.Points(i + SignalNewPointCountPerCollecting)
            SignalRawDataSeries.Points(i) = New DataPoint(PreviousData.X - SignalNewPointCountPerCollecting, PreviousData.Y)
            SignalFFTBuffer(i) = PreviousData.Y
        Next
        For i = SignalDataPointCount - SignalNewPointCountPerCollecting To SignalDataPointCount - 1
            Dim NewDataY As Double
            NewDataY = SignalSource(SignalSourceLinePointer)
            SignalRawDataSeries.Points(i) = New DataPoint(i, NewDataY)
            SignalFFTBuffer(i) = NewDataY * SignalFFTWindow(i)
            SignalSourceLinePointer = SignalSourceLinePointer + 1
            If SignalSourceLinePointer >= SignalSource.Count Then
                SignalSourceLinePointer = Int(GenerateRandomDouble(0, SignalSource.Count - 1))
            End If
        Next

        'Prepare FFT data padding
        For i = SignalDataPointCount To SignalFFTBufferSize - 1
            SignalFFTBuffer(i) = 0
        Next

        'Run FFT
        Fourier.ForwardReal(SignalFFTBuffer, SignalDataPointCount)

        'Complex result to real and displaying
        For i = 0 To SignalDataPointCount / 2 - 1
            SignalFFTDataSeries.Points(i) = New DataPoint(i, Math.Sqrt(SignalFFTBuffer(2 * i) ^ 2 + SignalFFTBuffer(2 * i + 1) ^ 2))
        Next

        'Update spectrogram
        For i = 0 To SignalDataPointCount / 2 - 1
            SpectrogramBuffer(CurrentSpectrogramColumn, i) = SignalFFTDataSeries.Points(i).Y
            SpectrogramBuffer((CurrentSpectrogramColumn + 1) Mod SpectrogramLineCount, i) = SignalFFTMin - 1
            'SpectrogramBuffer((CurrentSpectrogramColumn + 2) Mod SpectrogramLineCount, i) = SignalFFTMin - 1
        Next
        SpectrogramPlotter.Data = SpectrogramBuffer
        CurrentSpectrogramColumn = CurrentSpectrogramColumn + 1
        CurrentSpectrogramColumn = CurrentSpectrogramColumn Mod SpectrogramLineCount

        'Update display
        SignalRawDataSeries.PlotModel.InvalidatePlot(True)
        SignalFFTDataSeries.PlotModel.InvalidatePlot(True)
        SpectrogramDataSeries.PlotModel.InvalidatePlot(True)
    End Sub
End Class
