Imports System.Diagnostics
Imports System.Reflection
Imports System.IO
Imports System.Text
Imports System.Windows.Threading
Imports OxyPlot
Imports OxyPlot.Wpf

Public Class SystemMonitorWindow
    'Data collecting options
    Const HistoryDataPointCount As Integer = 100
    Const DataCollectingInterval As Integer = 500

    'CPU data buffer and timer
    'Performace counter and timer
    Dim CPUCounter As PerformanceCounter
    Dim CPUDataCollectingTimer As New DispatcherTimer(DispatcherPriority.Render)
    'Data plotting related
    Dim CPUHistoryPlotter As New LineSeries
    Dim CPUHistoryDataSeries As OxyPlot.Series.LineSeries
    Dim CPUHistoryXAxis As Axis
    Dim CPUHistoryYAxis As Axis

    'RAM data buffer and timer
    'Performace counter and timer
    Dim RAMCounter As New Microsoft.VisualBasic.Devices.ComputerInfo
    Dim RAMDataCollectingTimer As New DispatcherTimer(DispatcherPriority.Render)
    'Data plotting related
    Dim RAMHistoryPlotter As New LineSeries
    Dim RAMHistoryDataSeries As OxyPlot.Series.LineSeries
    Dim RAMHistoryXAxis As Axis
    Dim RAMHistoryYAxis As Axis

    'Disk data buffer and timer
    'Performace counter and timer
    Dim DiskReadCounter As PerformanceCounter
    Dim DiskWriteCounter As PerformanceCounter
    Dim DiskDataCollectingTimer As New DispatcherTimer(DispatcherPriority.Render)
    'Data plotting related
    Dim DiskReadHistoryPlotter As New LineSeries
    Dim DiskReadHistoryDataSeries As OxyPlot.Series.LineSeries
    Dim DiskWriteHistoryPlotter As New LineSeries
    Dim DiskWriteHistoryDataSeries As OxyPlot.Series.LineSeries
    Dim DiskHistoryXAxis As Axis
    Dim DiskHistoryYAxis As Axis

    Private Function GetCPUUsage() As Double
        Return CPUCounter.NextValue()
    End Function

    Private Function GetRAMUsage() As Double
        Return (RAMCounter.TotalPhysicalMemory - RAMCounter.AvailablePhysicalMemory) / 1024 / 1024
    End Function

    Private Function GetRAMTotalInMegabyte() As Double
        Return RAMCounter.TotalPhysicalMemory / 1024 / 1024
    End Function

    Private Function GetDiskReadLoad() As Double
        Return DiskReadCounter.NextValue()
    End Function
    Private Function GetDiskWriteLoad() As Double
        Return DiskWriteCounter.NextValue()
    End Function
    Private Sub SystemMonitorWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'CPU history
        'Performace counter and timer
        CPUCounter = New PerformanceCounter("Processor", "% Processor Time", "_Total")
        CPUCounter.NextValue()
        Dim CurrentCPUUsage As Double = GetCPUUsage()
        lblCPUUsage.Text = "CPU Usage - " & CurrentCPUUsage.ToString("F2") & "%"
        CPUDataCollectingTimer.Interval = TimeSpan.FromMilliseconds(DataCollectingInterval)
        AddHandler CPUDataCollectingTimer.Tick, AddressOf CPUDataCollectingTimer_Tick
        'Data plotting related
        'Plotting area
        CPUHistoryPlotter.Color = Color.FromRgb(0, 255, 0)
        'Data series
        CPUHistoryDataSeries = CPUHistoryPlotter.CreateModel()
        For i As Integer = 1 To HistoryDataPointCount
            CPUHistoryDataSeries.Points.Add(New DataPoint(i - 1, 0))
        Next
        'X-Axis
        CPUHistoryXAxis = New LinearAxis
        CPUHistoryXAxis.IsAxisVisible = True
        CPUHistoryXAxis.Layer = Axes.AxisLayer.AboveSeries
        CPUHistoryXAxis.AxislineStyle = LineStyle.Solid
        CPUHistoryXAxis.AxislineThickness = 1
        CPUHistoryXAxis.AxislineColor = Color.FromRgb(0, 255, 0)
        CPUHistoryXAxis.MajorGridlineStyle = LineStyle.Dash
        CPUHistoryXAxis.MajorGridlineThickness = 1
        CPUHistoryXAxis.MajorGridlineColor = Color.FromRgb(0, 150, 0)
        CPUHistoryXAxis.MajorStep = 20
        CPUHistoryXAxis.TextColor = Color.FromRgb(0, 255, 0)
        CPUHistoryXAxis.Font = "Consolas"
        CPUHistoryXAxis.LabelFormatter = Function(Val As Double) ""
        CPUHistoryXAxis.Position = Axes.AxisPosition.Bottom
        CPUHistoryXAxis.Maximum = HistoryDataPointCount - 1
        CPUHistoryXAxis.Minimum = 0
        CPUHistoryXAxis.IsZoomEnabled = False
        CPUHistoryXAxis.IsPanEnabled = False
        'Y-Axis
        CPUHistoryYAxis = New LinearAxis
        CPUHistoryYAxis.IsAxisVisible = True
        CPUHistoryYAxis.Layer = Axes.AxisLayer.AboveSeries
        CPUHistoryYAxis.AxislineStyle = LineStyle.Solid
        CPUHistoryYAxis.AxislineThickness = 1
        CPUHistoryYAxis.AxislineColor = Color.FromRgb(0, 255, 0)
        CPUHistoryYAxis.MajorGridlineStyle = LineStyle.Dash
        CPUHistoryYAxis.MajorGridlineThickness = 1
        CPUHistoryYAxis.MajorGridlineColor = Color.FromRgb(0, 150, 0)
        CPUHistoryYAxis.MajorStep = 25
        CPUHistoryYAxis.TextColor = Color.FromRgb(0, 255, 0)
        CPUHistoryYAxis.Font = "Consolas"
        CPUHistoryYAxis.LabelFormatter = Function(Val As Double) ""
        CPUHistoryYAxis.Position = Axes.AxisPosition.Left
        CPUHistoryYAxis.Maximum = 100 + 2
        CPUHistoryYAxis.Minimum = 0
        CPUHistoryYAxis.IsZoomEnabled = False
        CPUHistoryYAxis.IsPanEnabled = False
        'Layout updating
        pltCPUUsage.IsLegendVisible = False
        pltCPUUsage.TitlePadding = 0
        pltCPUUsage.Axes.Add(CPUHistoryXAxis)
        pltCPUUsage.Axes.Add(CPUHistoryYAxis)
        pltCPUUsage.Series.Add(CPUHistoryPlotter)

        'RAM history
        'Performace counter and timer
        'RAMCounter = New PerformanceCounter("Memory", "Available MBytes")
        'RAMCounter.NextValue()
        Dim CurrentRAMUsage As Double = GetRAMUsage()
        lblRAMUsage.Text = "RAM Usage - " & CurrentRAMUsage.ToString("F2") & " MB / " & GetRAMTotalInMegabyte().ToString("F2") & " MB"
        RAMDataCollectingTimer.Interval = TimeSpan.FromMilliseconds(DataCollectingInterval)
        AddHandler RAMDataCollectingTimer.Tick, AddressOf RAMDataCollectingTimer_Tick
        'Data plotting related
        'Plotting area
        RAMHistoryPlotter.Color = Color.FromRgb(0, 255, 0)
        'Data series
        RAMHistoryDataSeries = RAMHistoryPlotter.CreateModel()
        For i As Integer = 1 To HistoryDataPointCount
            RAMHistoryDataSeries.Points.Add(New DataPoint(i - 1, 0))
        Next
        'X-Axis
        RAMHistoryXAxis = New LinearAxis
        RAMHistoryXAxis.IsAxisVisible = True
        RAMHistoryXAxis.Layer = Axes.AxisLayer.AboveSeries
        RAMHistoryXAxis.AxislineStyle = LineStyle.Solid
        RAMHistoryXAxis.AxislineThickness = 1
        RAMHistoryXAxis.AxislineColor = Color.FromRgb(0, 255, 0)
        RAMHistoryXAxis.MajorGridlineStyle = LineStyle.Dash
        RAMHistoryXAxis.MajorGridlineThickness = 1
        RAMHistoryXAxis.MajorGridlineColor = Color.FromRgb(0, 150, 0)
        RAMHistoryXAxis.MajorStep = 20
        RAMHistoryXAxis.TextColor = Color.FromRgb(0, 255, 0)
        RAMHistoryXAxis.Font = "Consolas"
        RAMHistoryXAxis.LabelFormatter = Function(Val As Double) ""
        RAMHistoryXAxis.Position = Axes.AxisPosition.Bottom
        RAMHistoryXAxis.Maximum = HistoryDataPointCount - 1
        RAMHistoryXAxis.Minimum = 0
        RAMHistoryXAxis.IsZoomEnabled = False
        RAMHistoryXAxis.IsPanEnabled = False
        'Y-Axis
        RAMHistoryYAxis = New LinearAxis
        RAMHistoryYAxis.IsAxisVisible = True
        RAMHistoryYAxis.Layer = Axes.AxisLayer.AboveSeries
        RAMHistoryYAxis.AxislineStyle = LineStyle.Solid
        RAMHistoryYAxis.AxislineThickness = 1
        RAMHistoryYAxis.AxislineColor = Color.FromRgb(0, 255, 0)
        RAMHistoryYAxis.MajorGridlineStyle = LineStyle.Dash
        RAMHistoryYAxis.MajorGridlineThickness = 1
        RAMHistoryYAxis.MajorGridlineColor = Color.FromRgb(0, 150, 0)
        RAMHistoryYAxis.MajorStep = GetRAMTotalInMegabyte() / 4
        RAMHistoryYAxis.TextColor = Color.FromRgb(0, 255, 0)
        RAMHistoryYAxis.Font = "Consolas"
        RAMHistoryYAxis.LabelFormatter = Function(Val As Double) ""
        RAMHistoryYAxis.Position = Axes.AxisPosition.Left
        RAMHistoryYAxis.Maximum = GetRAMTotalInMegabyte() + 2
        RAMHistoryYAxis.Minimum = 0
        RAMHistoryYAxis.IsZoomEnabled = False
        RAMHistoryYAxis.IsPanEnabled = False
        'Layout updating
        pltRAMUsage.IsLegendVisible = False
        pltRAMUsage.TitlePadding = 0
        pltRAMUsage.Axes.Add(RAMHistoryXAxis)
        pltRAMUsage.Axes.Add(RAMHistoryYAxis)
        pltRAMUsage.Series.Add(RAMHistoryPlotter)

        'Disk history
        'Performace counter and timer
        DiskReadCounter = New PerformanceCounter("PhysicalDisk", "% Disk Read Time", "_Total")
        DiskWriteCounter = New PerformanceCounter("PhysicalDisk", "% Disk Write Time", "_Total")
        DiskReadCounter.NextValue()
        DiskWriteCounter.NextValue()
        Dim CurrentDiskReadLoad As Double = GetDiskReadLoad()
        Dim CurrentDiskWriteLoad As Double = GetDiskWriteLoad()
        lblDiskUsage.Text = "Disk Usage - " & CurrentDiskReadLoad.ToString("F2") & "% Read / " & CurrentDiskWriteLoad.ToString("F2") & "% Write"
        DiskDataCollectingTimer.Interval = TimeSpan.FromMilliseconds(DataCollectingInterval)
        AddHandler DiskDataCollectingTimer.Tick, AddressOf DiskDataCollectingTimer_Tick
        'Data plotting related
        'Plotting area
        DiskReadHistoryPlotter.Color = Color.FromRgb(0, 255, 0)
        DiskWriteHistoryPlotter.Color = Color.FromRgb(0, 255, 255)
        'Data series
        DiskReadHistoryDataSeries = DiskReadHistoryPlotter.CreateModel()
        DiskWriteHistoryDataSeries = DiskWriteHistoryPlotter.CreateModel()
        For i As Integer = 1 To HistoryDataPointCount
            DiskReadHistoryDataSeries.Points.Add(New DataPoint(i - 1, 0))
            DiskWriteHistoryDataSeries.Points.Add(New DataPoint(i - 1, 0))
        Next
        'X-Axis
        DiskHistoryXAxis = New LinearAxis
        DiskHistoryXAxis.IsAxisVisible = True
        DiskHistoryXAxis.Layer = Axes.AxisLayer.AboveSeries
        DiskHistoryXAxis.AxislineStyle = LineStyle.Solid
        DiskHistoryXAxis.AxislineThickness = 1
        DiskHistoryXAxis.AxislineColor = Color.FromRgb(0, 255, 0)
        DiskHistoryXAxis.MajorGridlineStyle = LineStyle.Dash
        DiskHistoryXAxis.MajorGridlineThickness = 1
        DiskHistoryXAxis.MajorGridlineColor = Color.FromRgb(0, 150, 0)
        DiskHistoryXAxis.MajorStep = 20
        DiskHistoryXAxis.TextColor = Color.FromRgb(0, 255, 0)
        DiskHistoryXAxis.Font = "Consolas"
        DiskHistoryXAxis.LabelFormatter = Function(Val As Double) ""
        DiskHistoryXAxis.Position = Axes.AxisPosition.Bottom
        DiskHistoryXAxis.Maximum = HistoryDataPointCount - 1
        DiskHistoryXAxis.Minimum = 0
        DiskHistoryXAxis.IsZoomEnabled = False
        DiskHistoryXAxis.IsPanEnabled = False
        'Y-Axis
        DiskHistoryYAxis = New LinearAxis
        DiskHistoryYAxis.IsAxisVisible = True
        DiskHistoryYAxis.Layer = Axes.AxisLayer.AboveSeries
        DiskHistoryYAxis.AxislineStyle = LineStyle.Solid
        DiskHistoryYAxis.AxislineThickness = 1
        DiskHistoryYAxis.AxislineColor = Color.FromRgb(0, 255, 0)
        DiskHistoryYAxis.MajorGridlineStyle = LineStyle.Dash
        DiskHistoryYAxis.MajorGridlineThickness = 1
        DiskHistoryYAxis.MajorGridlineColor = Color.FromRgb(0, 150, 0)
        DiskHistoryYAxis.MajorStep = 25
        DiskHistoryYAxis.TextColor = Color.FromRgb(0, 255, 0)
        DiskHistoryYAxis.Font = "Consolas"
        DiskHistoryYAxis.LabelFormatter = Function(Val As Double) ""
        DiskHistoryYAxis.Position = Axes.AxisPosition.Left
        DiskHistoryYAxis.Maximum = 100 + 2
        DiskHistoryYAxis.Minimum = 0
        DiskHistoryYAxis.IsZoomEnabled = False
        DiskHistoryYAxis.IsPanEnabled = False
        'Layout updating
        pltDiskUsage.IsLegendVisible = False
        pltDiskUsage.TitlePadding = 0
        pltDiskUsage.Axes.Add(DiskHistoryXAxis)
        pltDiskUsage.Axes.Add(DiskHistoryYAxis)
        pltDiskUsage.Series.Add(DiskReadHistoryPlotter)
        pltDiskUsage.Series.Add(DiskWriteHistoryPlotter)

        'Start timers
        CPUDataCollectingTimer.Start()
        RAMDataCollectingTimer.Start()
        DiskDataCollectingTimer.Start()
    End Sub

    Private Sub CPUDataCollectingTimer_Tick()
        'Move data forward
        For i As Integer = 0 To HistoryDataPointCount - 2
            Dim PreviousData As DataPoint
            PreviousData = CPUHistoryDataSeries.Points(i + 1)
            CPUHistoryDataSeries.Points(i) = New DataPoint(PreviousData.X - 1, PreviousData.Y)
        Next
        'Add newest data
        Dim CurrentCPUUsage As Double = GetCPUUsage()
        CPUHistoryDataSeries.Points(HistoryDataPointCount - 1) = New DataPoint(HistoryDataPointCount - 1, CurrentCPUUsage)
        CPUHistoryDataSeries.PlotModel.InvalidatePlot(True)
        lblCPUUsage.Text = "CPU Usage - " & CurrentCPUUsage.ToString("F2") & "%"
    End Sub

    Private Sub RAMDataCollectingTimer_Tick()
        'Move data forward
        For i As Integer = 0 To HistoryDataPointCount - 2
            Dim PreviousData As DataPoint
            PreviousData = RAMHistoryDataSeries.Points(i + 1)
            RAMHistoryDataSeries.Points(i) = New DataPoint(PreviousData.X - 1, PreviousData.Y)
        Next
        'Add newest data
        Dim CurrentRAMUsage As Double = GetRAMUsage()
        RAMHistoryDataSeries.Points(HistoryDataPointCount - 1) = New DataPoint(HistoryDataPointCount - 1, CurrentRAMUsage)
        RAMHistoryDataSeries.PlotModel.InvalidatePlot(True)
        lblRAMUsage.Text = "RAM Usage - " & CurrentRAMUsage.ToString("F2") & " MB / " & GetRAMTotalInMegabyte().ToString("F2") & " MB"
    End Sub

    Private Sub DiskDataCollectingTimer_Tick()
        'Move data forward
        For i As Integer = 0 To HistoryDataPointCount - 2
            Dim PreviousData As DataPoint
            PreviousData = DiskReadHistoryDataSeries.Points(i + 1)
            DiskReadHistoryDataSeries.Points(i) = New DataPoint(PreviousData.X - 1, PreviousData.Y)
            PreviousData = DiskWriteHistoryDataSeries.Points(i + 1)
            DiskWriteHistoryDataSeries.Points(i) = New DataPoint(PreviousData.X - 1, PreviousData.Y)
        Next
        'Add newest data
        Dim CurrentDiskReadLoad As Double = GetDiskReadLoad()
        Dim CurrentDiskWriteLoad As Double = GetDiskWriteLoad()
        DiskReadHistoryDataSeries.Points(HistoryDataPointCount - 1) = New DataPoint(HistoryDataPointCount - 1, CurrentDiskReadLoad)
        DiskReadHistoryDataSeries.PlotModel.InvalidatePlot(True)
        DiskWriteHistoryDataSeries.Points(HistoryDataPointCount - 1) = New DataPoint(HistoryDataPointCount - 1, CurrentDiskWriteLoad)
        DiskWriteHistoryDataSeries.PlotModel.InvalidatePlot(True)
        lblDiskUsage.Text = "Disk Usage - " & CurrentDiskReadLoad.ToString("F2") & "% Read / " & CurrentDiskWriteLoad.ToString("F2") & "% Write"
    End Sub
End Class
