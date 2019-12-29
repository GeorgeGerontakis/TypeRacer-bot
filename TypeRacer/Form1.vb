Public Class Form1
    Dim txt As String
    Dim charArray() As Char
    Function FocusTextField()
        Try
            Dim PageElement As HtmlElementCollection = WebBrowser1.Document.GetElementsByTagName("input")
            For Each CurElement As HtmlElement In PageElement
                If (CurElement.GetAttribute("autocorrect") = "off") Then
                    CurElement.Focus()
                    Exit For
                End If
            Next
        Catch
        End Try
    End Function
    Function getText() As String
        Try
            Dim PageElement As HtmlElementCollection = WebBrowser1.Document.GetElementsByTagName("span")
            For Each CurElement As HtmlElement In PageElement
                If (CurElement.GetAttribute("unselectable") = "on") Then
                    txt = txt & CurElement.InnerText
                End If
            Next
            Return txt
        Catch
        End Try
    End Function
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Timer1.Interval = NumericUpDown1.Value
        WebBrowser1.Document.Focus()
        System.Windows.Forms.SendKeys.Send("^%I")
        Timer2.Start()
        Button1.Enabled = False
    End Sub
    Dim i As Integer = 0
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Try
            System.Windows.Forms.SendKeys.Send(charArray(i))
            i = i + 1
        Catch
            Timer1.Stop()
            MsgBox("Finished!")
        End Try
    End Sub
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        Try
            Dim PageElement As HtmlElementCollection = WebBrowser1.Document.GetElementsByTagName("span")
            For Each CurElement As HtmlElement In PageElement
                If (CurElement.GetAttribute("title") = "Time remaining") Then
                    FocusTextField()
                    charArray = getText().ToCharArray
                    Timer1.Start()
                    Timer2.Stop()
                End If
            Next
        Catch
        End Try
    End Sub
End Class
