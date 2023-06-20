Imports System
Imports OWASP.WebGoat.NET.App_Code
Imports System.Collections.Generic
Imports System.Text

Namespace OWASP.WebGoat.NET.Content
    Partial Public Class Random
        Inherits System.Web.UI.Page

        Private Const MIN As UInteger = 1
        Private Const MAX As UInteger = 1000
        Private Const INIT_NUMBERS As Integer = 5

        Public Sub Page_Load(sender As Object, args As EventArgs)
            If Session("Random") Is Nothing Then Reset()

            Dim numbers As IList(Of UInteger) = CType(Session("Numbers"), IList(Of UInteger))
            lblSequence.Text = "Sequence: " & Print(numbers)
        End Sub

        Public Sub btnOneMore_Click(sender As Object, args As EventArgs)
            Dim rnd As WeakRandom = CType(Session("Random"), WeakRandom)
            Dim numbers As IList(Of UInteger) = CType(Session("Numbers"), IList(Of UInteger))

            numbers.Add(rnd.Next(MIN, MAX))

            lblSequence.Text = "Sequence: " & Print(numbers)
        End Sub

        Public Sub btnGo_Click(sender As Object, args As EventArgs)
            Dim rnd As WeakRandom = CType(Session("Random"), WeakRandom)

            Dim nextNumber As UInteger = rnd.Peek(MIN, MAX)

            If txtNextNumber.Text = nextNumber.ToString() Then
                lblResult.Text = "You found it!"
            Else
                lblResult.Text = "Sorry please try again."
            End If
        End Sub

        Public Sub btnReset_Click(sender As Object, args As EventArgs)
            Reset()

            Dim numbers As IList(Of UInteger) = CType(Session("Numbers"), IList(Of UInteger))
            lblSequence.Text = "Sequence: " & Print(numbers)
        End Sub

        Private Function Print(numbers As IList(Of UInteger)) As String
            Dim strBuilder As New StringBuilder()

            For Each n As UInteger In numbers
                strBuilder.AppendFormat("{0}, ", n)
            Next

            Return strBuilder.ToString()
        End Function

        Public Sub Reset()
            Session("Random") = New WeakRandom()

            Dim rnd As WeakRandom = CType(Session("Random"), WeakRandom)

            Dim numbers As IList(Of UInteger) = New List(Of UInteger)()

            For i As Integer = 0 To INIT_NUMBERS - 1
                numbers.Add(rnd.Next(MIN, MAX))
            Next

            Session("Numbers") = numbers
        End Sub
    End Class
End Namespace