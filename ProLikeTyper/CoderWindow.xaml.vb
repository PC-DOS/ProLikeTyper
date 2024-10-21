Imports System.Reflection
Imports System.IO
Imports System.Text
Imports System.Windows.Threading
Public Class CoderWindow
    'Timer object for "coding"
    Const TypingInterval As Double = 25
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

    'Console cursor
    Dim IsConsoleCursorVisible As Boolean = True
    Const ConsoleCursorFull As String = "▮"
    Const ConsoleCursorEmpty As String = "▯"

    Private Function GenerateRandomCodeLineNumber() As Integer
        Return RandomGen.Next(0, CodeSourceList.Count)
    End Function

    Private Sub CoderWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Try loading code from external code file
        Dim ExternalCodeFilePath As String = AppDomain.CurrentDomain.SetupInformation.ApplicationBase & "\" & ExternalCodeSource
        If File.Exists(ExternalCodeFilePath) Then
            Dim ExternalCodeFileStream As StreamReader = File.OpenText(ExternalCodeFilePath)
            While Not ExternalCodeFileStream.EndOfStream
                CodeSourceList.Add(ExternalCodeFileStream.ReadLine())
            End While
            ExternalCodeFileStream.Close()
        End If

        'If no external code file available, or code file is empty, fall back to internal code source
        If CodeSourceList.Count = 0 Then
            Dim InternalCodeFilePath As String = MethodBase.GetCurrentMethod().DeclaringType.Namespace & "." & InternalCodeSource
            Dim InternalCodeFileRawStream As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(InternalCodeFilePath)
            Dim InternalCodeFileStream As New StreamReader(InternalCodeFileRawStream)
            While Not InternalCodeFileStream.EndOfStream
                CodeSourceList.Add(InternalCodeFileStream.ReadLine())
            End While
            InternalCodeFileStream.Close()
        End If

        'Initialize code pointers
        CodeSourceLinePointer = GenerateRandomCodeLineNumber()
        CodeSourceColumnPointer = 0

        'Initialize timers
        AddHandler CoderTimer.Tick, AddressOf CoderTimer_Tick
        CoderTimer.Interval = TimeSpan.FromMilliseconds(TypingInterval)
        CoderTimer.Start()
        AddHandler CursorTimer.Tick, AddressOf CursorTimer_Tick
        CursorTimer.Interval = TimeSpan.FromMilliseconds(CursorFalshingInterval)
        CursorTimer.Start()
    End Sub

    Private Sub CoderTimer_Tick()
        ''Testing: generate a random text from ASCII table
        'Dim CharAscii As Integer = 0
        'CharAscii = RandomGen.Next(32, 129)
        'If CharAscii <> 128 Then
        '    CodeText = CodeText & Chr(CharAscii)
        'Else
        '    CodeText = CodeText & vbCrLf
        'End If

        'Load next character from code source
        If CodeSourceList(CodeSourceLinePointer).Length > 0 Then
            CodeText = CodeText & CodeSourceList(CodeSourceLinePointer)(CodeSourceColumnPointer)
        End If
        'Update line & column pointers
        CodeSourceColumnPointer += 1
        If CodeSourceColumnPointer >= CodeSourceList(CodeSourceLinePointer).Length Then
            'Add Enter key
            CodeText = CodeText & vbCrLf

            'Change to new row
            CodeSourceColumnPointer = 0
            CodeSourceLinePointer += 1
            If CodeSourceLinePointer >= CodeSourceList.Count Then
                CodeSourceLinePointer = GenerateRandomCodeLineNumber()
            End If
        End If

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
        'lblCode.Text = CodeTextDisplay
    End Sub
End Class
