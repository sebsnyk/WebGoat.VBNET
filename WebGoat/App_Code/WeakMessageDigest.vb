Imports System
Imports System.Text
Imports log4net
Imports System.Reflection

Namespace OWASP.WebGoat.NET.App_Code
    Public Class WeakMessageDigest
        Private Shared ReadOnly ascii As New ASCIIEncoding()
        
        Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

        Public Shared Function GenerateWeakDigest(ByVal msg As String) As String
            Dim tokens As String() = msg.Split(" "c)

            Dim bytes(tokens.Length - 1) As Byte

            For i As Integer = 0 To tokens.Length - 1
                Dim token As String = tokens(i)
                bytes(i) = GenByte(token)
            Next

            log.Debug(String.Format("Bytes for {0}...", msg))
            log.Debug(Print(bytes))

            Return ascii.GetString(bytes)
        End Function

        'Algo is dead simple. Just sum up the ASCII value and mod back to a printable char.
        Public Shared Function GenByte(ByVal word As String) As Byte
            Dim val As Integer = 0
            Dim bVal As Byte

            For Each c As Char In word
                val += CByte(c)
            Next

            'NOTE: Need to be between 32 and 126 in the ASCII table to be printable
            bVal = CByte((val Mod (127 - 32 - 1) + 33))

            Return bVal
        End Function

        Private Shared Function Print(ByVal bytes As Byte()) As String
            Dim strBuild As New StringBuilder()

            For Each b As Byte In bytes
                strBuild.AppendFormat("{0},", b)
            Next

            Return strBuild.ToString()
        End Function
    End Class

End Namespace