

Imports System.Text
Imports System.IO
Imports System.Net

Namespace CaptchaClient
    Public Class CaptchaSolver
#Region "Attributes"
        Private _PostUrl As String
        Private _AccessKey As String
        Private _VendorKey As String
        Private _LastResponseText As String
        Private _LastTaskId As String
        Private _LastPostState As ResponseState
#End Region

#Region "AttributeMehodes"
        Public ReadOnly Property LastTaskId() As String
            Get
                Return _LastTaskId
            End Get
        End Property
        Public ReadOnly Property LastPostState() As ResponseState
            Get
                Return _LastPostState
            End Get
        End Property
        Public ReadOnly Property LastResponseText() As String
            Get
                Return _LastResponseText
            End Get
        End Property
        Public ReadOnly Property AccessKey() As String
            Get
                Return _AccessKey
            End Get
        End Property
        Public ReadOnly Property VendorKey() As String
            Get
                Return _VendorKey
            End Get
        End Property
        Public ReadOnly Property PostUrl() As String
            Get
                Return _PostUrl
            End Get
        End Property
#End Region

        Public Sub New(ImagePostUrl As String, Key As String, PartnerVendorKey As String)
            Me._PostUrl = ImagePostUrl
            Me._AccessKey = Key
            Me._VendorKey = PartnerVendorKey
        End Sub
        Public Sub New(ImagePostUrl As String, Key As String)
            Me._PostUrl = ImagePostUrl
            Me._AccessKey = Key
            Me._VendorKey = ""
        End Sub
#Region "PrivateMethods"
        Private Function EncodeUrl(str As String) As String
            If str Is Nothing Then
                Return ""
            End If

            Dim enc As Encoding = Encoding.ASCII
            Dim result As New StringBuilder()

            For Each symbol As Char In str
                Dim bs As Byte() = enc.GetBytes(New Char() {symbol})
                For i As Integer = 0 To bs.Length - 1
                    Dim b As Byte = bs(i)
                    If b >= 48 AndAlso b < 58 OrElse b >= 65 AndAlso b < 65 + 26 OrElse b >= 97 AndAlso b < 97 + 26 Then
                        ' decode non numalphabet
                        result.Append(Encoding.ASCII.GetString(bs, i, 1))
                    Else
                        result.Append("%"c + [String].Format("{0:X2}", CInt(b)))
                    End If
                Next
            Next

            Return result.ToString()
        End Function
        Private Sub Post(ParamArray ps As String())
            Try
                Me._LastResponseText = ""
                Dim request As HttpWebRequest = DirectCast(WebRequest.Create(Me.PostUrl), HttpWebRequest)
                request.Proxy = WebRequest.DefaultWebProxy
                Dim str As String = ""

                Dim i As Integer = 0
                While i + 1 < ps.Length
                    str += (Convert.ToString(EncodeUrl(ps(i)) & Convert.ToString("=")) & EncodeUrl(ps(i + 1))) + "&"
                    i += 2
                End While
                If str.EndsWith("&") Then
                    str = str.Substring(0, str.Length - 1)
                End If

                request.Method = "POST"
                request.ContentType = "application/x-www-form-urlencoded"
                Dim buffer As Byte() = Encoding.ASCII.GetBytes(str)
                request.ContentLength = buffer.Length
                Dim newStream As Stream = request.GetRequestStream()
                newStream.Write(buffer, 0, buffer.Length)

                Dim response As WebResponse = request.GetResponse()
                Dim sStream As Stream = response.GetResponseStream()
                Dim reader As New StreamReader(sStream)
                Dim ResponseSt As String = reader.ReadToEnd()
                Me.DecodeResponse(ResponseSt)
                reader.Close()
                response.Close()

                newStream.Close()
            Catch
                Me._LastResponseText = ""
            End Try
        End Sub


        Private Sub DecodeResponse(ResponseSt As String)
            Me._LastResponseText = ""
            Dim split As Char() = New Char() {" "c}
            Dim splitMessage As String() = ResponseSt.Split(split, StringSplitOptions.RemoveEmptyEntries)
            If splitMessage.Length > 1 AndAlso splitMessage(0) = "Error" Then

                Select Case splitMessage(1)
                    Case "INCORRECT_ID"
                        Me._LastPostState = ResponseState.INCORRECT_ACCESS_KEY
                        Exit Select
                    Case "NOT_ENOUGH_FUND"
                        Me._LastPostState = ResponseState.NOT_ENOUGH_FUND
                        Exit Select
                    Case "TIMEOUT"
                        Me._LastPostState = ResponseState.TIMEOUT
                        Exit Select
                    Case "INVALID_REQUEST"
                        Me._LastPostState = ResponseState.INVALID_REQUEST
                        Exit Select
                    Case "UNKNOWN"
                        Me._LastPostState = ResponseState.UNKNOWN
                        Exit Select
                    Case Else
                        Me._LastPostState = ResponseState.OK
                        Exit Select

                End Select
            Else
                Me._LastPostState = ResponseState.OK
                Me._LastResponseText = ResponseSt
            End If


        End Sub
#End Region
#Region "PublicMethods"
        Public Function SolveCaptcha(imageFileLocation As String) As Boolean
            Me._LastTaskId = Guid.NewGuid().ToString()

            ' read image data
            Dim buffer As Byte() = File.ReadAllBytes(imageFileLocation)

            ' base64 encode it
            Dim img As String = Convert.ToBase64String(buffer)

            ' submit captcha to server
            Post(New String() {"action", "upload", "vendorkey", Me.VendorKey, "key", Me.AccessKey,
                "file", img, "gen_task_id", Me._LastTaskId})

            If Me.LastPostState = ResponseState.OK Then
                Return True
            Else
                Return False
            End If
        End Function
        Public Function RefundLastTask() As Boolean
            Return Refund(Me.LastTaskId)
        End Function
        Public Function Refund(task_id As String) As Boolean

            Post(New String() {"action", "refund", "key", Me.AccessKey, "gen_task_id", task_id})

            If Me.LastPostState = ResponseState.OK Then
                Return True
            Else
                Return False
            End If

        End Function
        Public Function Balance() As Integer
            Try

                Post(New String() {"action", "balance", "key", Me.AccessKey})
                If Me.LastPostState = ResponseState.OK Then
                    Return Convert.ToInt32(Me.LastResponseText)
                Else
                    Return 0
                End If
            Catch
                Throw
            End Try
        End Function
#End Region
    End Class
    Public Enum ResponseState
        OK
        TIMEOUT
        INVALID_REQUEST
        INCORRECT_ACCESS_KEY
        NOT_ENOUGH_FUND
        UNKNOWN
    End Enum
End Namespace

'=======================================================
'Service provided by Telerik (www.telerik.com)
'Conversion powered by NRefactory.
'Twitter: @telerik
'Facebook: facebook.com/telerik
'=======================================================
