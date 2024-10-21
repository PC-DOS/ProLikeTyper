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
                    Dim CoderWindowInstance As New CoderWindow
                    CoderWindowInstance.Show()
                Case "NETSCAN"

                Case "SYSMON"
                    Dim SysMonWindowInstance As New SystemMonitorWindow
                    SysMonWindowInstance.Show()
                Case "PULLDATA"
                    Dim DownloaderWindowInstance As New DownloaderWindow
                    DownloaderWindowInstance.Show()
                Case "PRODMON"
                    Dim ProductionMonitorWindowInstance As New ProductionMonitorWindow
                    ProductionMonitorWindowInstance.Show()
                Case "COMPILE"
                    Dim CompilerWindowInstance As New CompilerWindow
                    CompilerWindowInstance.Show()
                Case "TRAIN"
                    Dim TrainWindowInstance As New TrainWindow
                    TrainWindowInstance.Show()
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
