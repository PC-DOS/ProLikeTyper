Imports System.Threading
Imports System.Threading.Tasks
Class MainWindow
    Const CommandNameDefault As String = "!UnnamedCommand!"
    Const CommandInternalNameDefault As String = "!NOCMDNAME!"
    Const CommandDescriptionNoSelection As String = "Double-click on an item to execute it"
    Const CommandDescriptionDefault As String = "No available descriptions for this command"

    Private Structure CommandItem
        Dim Name As String
        Dim InternalName As String
        Dim Description As String
    End Structure

    Private Function OpenWindowAsync(Of TWindow As {System.Windows.Window, New})() As Task
        Dim TaskComp As New TaskCompletionSource(Of Object)

        'Create child window container
        Dim WinThread As New Thread(Sub()
                                        Dim WindowInstance As New TWindow
                                        'Close target window's event loop when closed
                                        AddHandler WindowInstance.Closed, Sub()
                                                                              System.Windows.Threading.Dispatcher.ExitAllFrames()
                                                                          End Sub
                                        'Show window in separated thread
                                        WindowInstance.Show()
                                        'Start window's event loop
                                        System.Windows.Threading.Dispatcher.Run()
                                        'Set tasks result
                                        TaskComp.SetResult(Nothing)
                                    End Sub)

        'Allow sub thread to be exited when calling Application.Current.Shutdown()
        WinThread.IsBackground = True
        'Allow UI dispatching
        WinThread.SetApartmentState(ApartmentState.STA)
        'Start sub thread
        WinThread.Start()

        'Returns task object
        Return TaskComp.Task
    End Function

    Private Function ParseSelectedCommand(SelectedCommand As ListBoxItem) As CommandItem
        Dim CommandData As CommandItem
        'Split selected item tag by "_", tag is organized in following structure:
        'CommandInternalName_Description
        Dim SelectedCommandTags As String() = Split(SelectedCommand.Tag.ToString(), "_")
        Dim SelectedCommandDescription As String = ""
        Dim SelectedCommandName As String = SelectedCommand.Content.ToString().Trim()
        Dim SelectedCommandInternalName As String = SelectedCommandTags(0).Trim()

        'Restore potential underline(s) in description
        If UBound(SelectedCommandTags) >= 1 Then
            For i As Integer = 1 To UBound(SelectedCommandTags)
                SelectedCommandDescription = SelectedCommandDescription & SelectedCommandTags(i)
                If i <> UBound(SelectedCommandTags) Then
                    SelectedCommandDescription = SelectedCommandDescription & "_"
                End If
            Next
        End If

        'Apply default data if necessary
        If SelectedCommandName.Length = 0 Then
            SelectedCommandName = CommandNameDefault
        End If
        If SelectedCommandInternalName.Length = 0 Then
            SelectedCommandInternalName = CommandInternalNameDefault
        End If
        SelectedCommandDescription = SelectedCommandDescription.Trim()
        If SelectedCommandDescription.Length = 0 Then
            SelectedCommandDescription = CommandDescriptionDefault
        End If

        'Return data
        With CommandData
            .Name = SelectedCommandName
            .InternalName = SelectedCommandInternalName.ToUpper
            .Description = SelectedCommandDescription
        End With
        Return CommandData
    End Function

    Private Sub lstCommands_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles lstCommands.MouseDoubleClick
        If lstCommands.SelectedIndex >= 0 Then
            'Parse selected item
            Dim SelectedCommandData As CommandItem = ParseSelectedCommand(lstCommands.SelectedItem)

            'Display command description
            lblCommandDescription.Text = SelectedCommandData.Name & " (" & SelectedCommandData.InternalName & ")" & vbCrLf & SelectedCommandData.Description

            'Execute command
            Select Case SelectedCommandData.InternalName
                Case "CODER"
                    OpenWindowAsync(Of CoderWindow)()
                Case "NETSCAN"
                    OpenWindowAsync(Of NetScanWindow)()
                Case "SYSMON"
                    OpenWindowAsync(Of SystemMonitorWindow)()
                Case "PULLDATA"
                    OpenWindowAsync(Of DownloaderWindow)()
                Case "PRODMON"
                    OpenWindowAsync(Of ProductionMonitorWindow)()
                Case "COMPILE"
                    OpenWindowAsync(Of CompilerWindow)()
                Case "TRAIN"
                    OpenWindowAsync(Of TrainWindow)()
                Case "EXIT"
                    Application.Current.Shutdown()
                Case Else
                    'Do nothing currently
            End Select
        Else
            lblCommandDescription.Text = CommandDescriptionNoSelection
        End If
    End Sub

    Private Sub lstCommands_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lstCommands.SelectionChanged
        If lstCommands.SelectedIndex >= 0 Then
            'Parse selected item
            Dim SelectedCommandData As CommandItem = ParseSelectedCommand(lstCommands.SelectedItem)

            'Display command description
            lblCommandDescription.Text = SelectedCommandData.Name & " (" & SelectedCommandData.InternalName & ")" & vbCrLf & SelectedCommandData.Description
        Else
            lblCommandDescription.Text = CommandDescriptionNoSelection
        End If
    End Sub
End Class
