Imports System.Reflection
Imports System.IO
Imports System.Text
Imports System.Windows.Threading
Public Class ProductionMonitorWindow
    'Value constants
    Const PipePressureMin As Double = 0
    Const PipePressureMax As Double = 2.45
    Const PipePressureOffset As Double = 0.01
    Const ProdSpeedMin As Double = 0
    Const ProdSpeedMax As Double = 2450
    Const ProdSpeedOffset As Double = 5
    Const StoreMin As Double = 0
    Const StoreMax As Double = 50
    Const StoreOffset As Double = 1

    'Global randowm generator
    Dim RandomGen As New Random

    'Timers
    Dim DataUpdateTimer As New DispatcherTimer
    Private Function GenerateRandomDouble(ValMin As Double, ValMax As Double) As Double
        Return RandomGen.NextDouble() * (ValMax - ValMin) + ValMin
    End Function
    Private Sub RefreshData()
        'Date & time
        lblTime.Text = Date.Now.ToString("yyyy-MM-dd") & vbCrLf & Date.Now.ToString("HH:mm:ss")

        'Pressure
        prgPipeRed.Minimum = PipePressureMin
        prgPipeRed.Maximum = PipePressureMax
        prgPipeRed.Value += GenerateRandomDouble(-PipePressureOffset, PipePressureOffset)
        lblPipeRed.Text = prgPipeRed.Value.ToString("F2") & " MPa"
        prgPipeOrange.Minimum = PipePressureMin
        prgPipeOrange.Maximum = PipePressureMax
        prgPipeOrange.Value += GenerateRandomDouble(-PipePressureOffset, PipePressureOffset)
        lblPipeOrange.Text = prgPipeOrange.Value.ToString("F2") & " MPa"
        prgPipeYellow.Minimum = PipePressureMin
        prgPipeYellow.Maximum = PipePressureMax
        prgPipeYellow.Value += GenerateRandomDouble(-PipePressureOffset, PipePressureOffset)
        lblPipeYellow.Text = prgPipeYellow.Value.ToString("F2") & " MPa"
        prgPipeGreen.Minimum = PipePressureMin
        prgPipeGreen.Maximum = PipePressureMax
        prgPipeGreen.Value += GenerateRandomDouble(-PipePressureOffset, PipePressureOffset)
        lblPipeGreen.Text = prgPipeGreen.Value.ToString("F2") & " MPa"
        prgPipeCyan.Minimum = PipePressureMin
        prgPipeCyan.Maximum = PipePressureMax
        prgPipeCyan.Value += GenerateRandomDouble(-PipePressureOffset, PipePressureOffset)
        lblPipeCyan.Text = prgPipeCyan.Value.ToString("F2") & " MPa"
        prgPipeBlue.Minimum = PipePressureMin
        prgPipeBlue.Maximum = PipePressureMax
        prgPipeBlue.Value += GenerateRandomDouble(-PipePressureOffset, PipePressureOffset)
        lblPipeBlue.Text = prgPipeBlue.Value.ToString("F2") & " MPa"
        prgPipePurple.Minimum = PipePressureMin
        prgPipePurple.Maximum = PipePressureMax
        prgPipePurple.Value += GenerateRandomDouble(-PipePressureOffset, PipePressureOffset)
        lblPipePurple.Text = prgPipePurple.Value.ToString("F2") & " MPa"

        'Prod speed
        prgProd01.Minimum = ProdSpeedMin
        prgProd01.Maximum = ProdSpeedMax
        prgProd01.Value += GenerateRandomDouble(-ProdSpeedOffset, ProdSpeedOffset)
        lblProd01.Text = Math.Round(prgProd01.Value).ToString() & " RPM"
        prgProd02.Minimum = ProdSpeedMin
        prgProd02.Maximum = ProdSpeedMax
        prgProd02.Value += GenerateRandomDouble(-ProdSpeedOffset, ProdSpeedOffset)
        lblProd02.Text = Math.Round(prgProd02.Value).ToString() & " RPM"
        prgProd03.Minimum = ProdSpeedMin
        prgProd03.Maximum = ProdSpeedMax
        prgProd03.Value += GenerateRandomDouble(-ProdSpeedOffset, ProdSpeedOffset)
        lblProd03.Text = Math.Round(prgProd03.Value).ToString() & " RPM"
        prgProd04.Minimum = ProdSpeedMin
        prgProd04.Maximum = ProdSpeedMax
        prgProd04.Value += GenerateRandomDouble(-ProdSpeedOffset, ProdSpeedOffset)
        lblProd04.Text = Math.Round(prgProd04.Value).ToString() & " RPM"
        prgProd05.Minimum = ProdSpeedMin
        prgProd05.Maximum = ProdSpeedMax
        prgProd05.Value += GenerateRandomDouble(-ProdSpeedOffset, ProdSpeedOffset)
        lblProd05.Text = Math.Round(prgProd05.Value).ToString() & " RPM"

        'Store
        Dim StoreIn As Double
        Dim StoreOut As Double
        StoreIn = RandomGen.NextDouble() * StoreOffset
        StoreOut = RandomGen.NextDouble() * StoreOffset
        prgStoreRed.Minimum = StoreMin
        prgStoreRed.Maximum = StoreMax
        prgStoreRed.Value += StoreIn - StoreOut
        lblStoreRedIn.Text = "+" & StoreIn.ToString("F3")
        lblStoreRedOut.Text = "-" & StoreOut.ToString("F3")
        StoreIn = RandomGen.NextDouble() * StoreOffset
        StoreOut = RandomGen.NextDouble() * StoreOffset
        prgStoreOrange.Minimum = StoreMin
        prgStoreOrange.Maximum = StoreMax
        prgStoreOrange.Value += StoreIn - StoreOut
        lblStoreOrangeIn.Text = "+" & StoreIn.ToString("F3")
        lblStoreOrangeOut.Text = "-" & StoreOut.ToString("F3")
        StoreIn = RandomGen.NextDouble() * StoreOffset
        StoreOut = RandomGen.NextDouble() * StoreOffset
        prgStoreYellow.Minimum = StoreMin
        prgStoreYellow.Maximum = StoreMax
        prgStoreYellow.Value += StoreIn - StoreOut
        lblStoreYellowIn.Text = "+" & StoreIn.ToString("F3")
        lblStoreYellowOut.Text = "-" & StoreOut.ToString("F3")
        StoreIn = RandomGen.NextDouble() * StoreOffset
        StoreOut = RandomGen.NextDouble() * StoreOffset
        prgStoreGreen.Minimum = StoreMin
        prgStoreGreen.Maximum = StoreMax
        prgStoreGreen.Value += StoreIn - StoreOut
        lblStoreGreenIn.Text = "+" & StoreIn.ToString("F3")
        lblStoreGreenOut.Text = "-" & StoreOut.ToString("F3")
        StoreIn = RandomGen.NextDouble() * StoreOffset
        StoreOut = RandomGen.NextDouble() * StoreOffset
        prgStoreCyan.Minimum = StoreMin
        prgStoreCyan.Maximum = StoreMax
        prgStoreCyan.Value += StoreIn - StoreOut
        lblStoreCyanIn.Text = "+" & StoreIn.ToString("F3")
        lblStoreCyanOut.Text = "-" & StoreOut.ToString("F3")
        StoreIn = RandomGen.NextDouble() * StoreOffset
        StoreOut = RandomGen.NextDouble() * StoreOffset
        prgStoreBlue.Minimum = StoreMin
        prgStoreBlue.Maximum = StoreMax
        prgStoreBlue.Value += StoreIn - StoreOut
        lblStoreBlueIn.Text = "+" & StoreIn.ToString("F3")
        lblStoreBlueOut.Text = "-" & StoreOut.ToString("F3")
        StoreIn = RandomGen.NextDouble() * StoreOffset
        StoreOut = RandomGen.NextDouble() * StoreOffset
        prgStorePurple.Minimum = StoreMin
        prgStorePurple.Maximum = StoreMax
        prgStorePurple.Value += StoreIn - StoreOut
        lblStorePurpleIn.Text = "+" & StoreIn.ToString("F3")
        lblStorePurpleOut.Text = "-" & StoreOut.ToString("F3")
    End Sub
    Private Sub ProductionMonitorWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Initialize data
        RefreshData()

        'Generate initial data
        'Pressure
        prgPipeRed.Value = GenerateRandomDouble((PipePressureMax - PipePressureMin) * 0.25, (PipePressureMax - PipePressureMin) * 0.75)
        prgPipeOrange.Value = GenerateRandomDouble((PipePressureMax - PipePressureMin) * 0.25, (PipePressureMax - PipePressureMin) * 0.75)
        prgPipeYellow.Value = GenerateRandomDouble((PipePressureMax - PipePressureMin) * 0.25, (PipePressureMax - PipePressureMin) * 0.75)
        prgPipeGreen.Value = GenerateRandomDouble((PipePressureMax - PipePressureMin) * 0.25, (PipePressureMax - PipePressureMin) * 0.75)
        prgPipeCyan.Value = GenerateRandomDouble((PipePressureMax - PipePressureMin) * 0.25, (PipePressureMax - PipePressureMin) * 0.75)
        prgPipeBlue.Value = GenerateRandomDouble((PipePressureMax - PipePressureMin) * 0.25, (PipePressureMax - PipePressureMin) * 0.75)
        prgPipePurple.Value = GenerateRandomDouble((PipePressureMax - PipePressureMin) * 0.25, (PipePressureMax - PipePressureMin) * 0.75)
        'Prod speed
        prgProd01.Value = RandomGen.Next((ProdSpeedMax - ProdSpeedMin) * 0.25, (ProdSpeedMax - ProdSpeedMin) * 0.75)
        prgProd02.Value = RandomGen.Next((ProdSpeedMax - ProdSpeedMin) * 0.25, (ProdSpeedMax - ProdSpeedMin) * 0.75)
        prgProd03.Value = RandomGen.Next((ProdSpeedMax - ProdSpeedMin) * 0.25, (ProdSpeedMax - ProdSpeedMin) * 0.75)
        prgProd04.Value = RandomGen.Next((ProdSpeedMax - ProdSpeedMin) * 0.25, (ProdSpeedMax - ProdSpeedMin) * 0.75)
        prgProd05.Value = RandomGen.Next((ProdSpeedMax - ProdSpeedMin) * 0.25, (ProdSpeedMax - ProdSpeedMin) * 0.75)
        'Store
        prgStoreRed.Value = GenerateRandomDouble((StoreMax - StoreMin) * 0.25, (StoreMax - StoreMin) * 0.75)
        prgStoreOrange.Value = GenerateRandomDouble((StoreMax - StoreMin) * 0.25, (StoreMax - StoreMin) * 0.75)
        prgStoreYellow.Value = GenerateRandomDouble((StoreMax - StoreMin) * 0.25, (StoreMax - StoreMin) * 0.75)
        prgStoreGreen.Value = GenerateRandomDouble((StoreMax - StoreMin) * 0.25, (StoreMax - StoreMin) * 0.75)
        prgStoreCyan.Value = GenerateRandomDouble((StoreMax - StoreMin) * 0.25, (StoreMax - StoreMin) * 0.75)
        prgStoreBlue.Value = GenerateRandomDouble((StoreMax - StoreMin) * 0.25, (StoreMax - StoreMin) * 0.75)
        prgStorePurple.Value = GenerateRandomDouble((StoreMax - StoreMin) * 0.25, (StoreMax - StoreMin) * 0.75)

        'Refresh all data
        RefreshData()

        'Register timer
        AddHandler DataUpdateTimer.Tick, AddressOf RefreshData
        DataUpdateTimer.Interval = TimeSpan.FromMilliseconds(500)
        DataUpdateTimer.Start()
    End Sub
End Class
