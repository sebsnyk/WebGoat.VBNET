Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Data
Imports System.IO
Imports System.Text
Imports System.Configuration
Imports Mono.Data.Sqlite

Namespace OWASP.WebGoat.NET
    Public Class DatabaseUtilities
        Private conn As SqliteConnection = Nothing
        Private GoatDBFile As String = HttpContext.Current.Server.MapPath("~/App_Data/") & "goatdb.sqlite"

        Private Function GetGoatDBConnection() As SqliteConnection
            If conn Is Nothing Then
                Dim connectionstring As String = "Data Source=" & GoatDBFile

                conn = New SqliteConnection(connectionstring)
                conn.Open()
            End If
            Return conn
        End Function

        Public Function RecreateGoatDB() As Boolean
            If File.Exists(GoatDBFile) Then
                File.Delete(GoatDBFile)
            End If

            SqliteConnection.CreateFile(GoatDBFile)
            Dim cn As SqliteConnection = GetGoatDBConnection()
            CreateTables(cn)
            AddDataToTables(cn)

            cn.Close()
            Return True
        End Function

        Public Sub RunSQLFromFile(ByVal cn As SqliteConnection, ByVal filename As String)
            Using fs As New FileStream(filename, FileMode.Open)
                Using sr As New StreamReader(fs, Encoding.UTF8)
                    Dim line As String = String.Empty
                    While (InlineAssignHelper(line, sr.ReadLine())) IsNot Nothing
                        line = line.Trim()
                        If line.StartsWith("--") Then
                            Continue While
                        End If
                        DoNonQuery(line, cn)
                    End While
                End Using
            End Using
        End Sub

        Public Sub CreateTables(ByVal cn As SqliteConnection)
            Dim filename As String = HttpContext.Current.Server.MapPath("~/App_Data/") & "tables.sql"
            RunSQLFromFile(cn, filename)
        End Sub

        Public Sub AddDataToTables(ByVal cn As SqliteConnection)
            Dim filename As String = HttpContext.Current.Server.MapPath("~/App_Data/") & "tabledata.sql"
            RunSQLFromFile(cn, filename)
        End Sub

        Private Function DoNonQuery(ByVal SQL As String, ByVal conn As SqliteConnection) As String
            Dim cmd As New SqliteCommand(SQL, conn)
            Dim output As String = String.Empty

            Try
                cmd.ExecuteNonQuery()
                output &= "<br/>SQL Executed: " & SQL
            Catch ex As SqliteException
                output &= "<br/>SQL Exception: " & ex.Message
                output &= SQL
            Catch ex As Exception
                output &= "<br/>Exception: " & ex.Message
                output &= SQL
            End Try
            Return output
        End Function

        Private Function DoScalar(ByVal SQL As String, ByVal conn As SqliteConnection) As String
            Dim cmd As New SqliteCommand(SQL, conn)
            Dim output As String = String.Empty

            Try
                output = DirectCast(cmd.ExecuteScalar(), String)
            Catch ex As SqliteException
                output &= "<br/>SQL Exception: " & ex.Message & " - "
                output &= SQL
            Catch ex As Exception
                output &= "<br/>Exception: " & ex.Message & " - "
                output &= SQL
            End Try
            Return output
        End Function

        Private Function DoQuery(ByVal SQL As String, ByVal conn As SqliteConnection) As DataTable
            Dim cmd As New SqliteCommand(SQL, conn)
            Dim dt As New DataTable()

            Using reader As var = cmd.ExecuteReader()

                For i As Integer = 0 To reader.FieldCount - 1
                    Dim col As New DataColumn()
                    col.DataType = reader.GetFieldType(i)
                    col.ColumnName = reader.GetName(i)
                    dt.Columns.Add(col)
                Next

                While reader.Read()
                    Dim row As DataRow = dt.NewRow()

                    For i As Integer = 0 To reader.FieldCount - 1
                        If reader.IsDBNull(i) Then
                            Continue For
                        End If

                        If reader.GetFieldType(i) = GetType(String) Then
                            row(dt.Columns(i).ColumnName) = reader.GetString(i)
                        ElseIf reader.GetFieldType(i) = GetType(Int16) Then
                            row(dt.Columns(i).ColumnName) = reader.GetInt16(i)
                        ElseIf reader.GetFieldType(i) = GetType(Int32) Then
                            row(dt.Columns(i).ColumnName) = reader.GetInt32(i)
                        ElseIf reader.GetFieldType(i) = GetType(Int64) Then
                            row(dt.Columns(i).ColumnName) = reader.GetInt64(i)
                        ElseIf reader.GetFieldType(i) = GetType(Boolean) Then
                            row(dt.Columns(i).ColumnName) = reader.GetBoolean(i)
                        ElseIf reader.GetFieldType(i) = GetType(Byte) Then
                            row(dt.Columns(i).ColumnName) = reader.GetByte(i)
                        ElseIf reader.GetFieldType(i) = GetType(Char) Then
                            row(dt.Columns(i).ColumnName) = reader.GetChar(i)
                        ElseIf reader.GetFieldType(i) = GetType(DateTime) Then
                            row(dt.Columns(i).ColumnName) = reader.GetDateTime(i)
                        ElseIf reader.GetFieldType(i) = GetType(Decimal) Then
                            row(dt.Columns(i).ColumnName) = reader.GetDecimal(i)
                        ElseIf reader.GetFieldType(i) = GetType(Double) Then
                            row(dt.Columns(i).ColumnName) = reader.GetDouble(i)
                        ElseIf reader.GetFieldType(i) = GetType(Single) Then
                            row(dt.Columns(i).ColumnName) = reader.GetFloat(i)
                        ElseIf reader.GetFieldType(i) = GetType(Guid) Then
                            row(dt.Columns(i).ColumnName) = reader.GetGuid(i)
                        End If
                    Next

                    dt.Rows.Add(row)
                End While
            End Using

            Return dt
        End Function

        Public Function GetEmailByUserID(ByVal userid As String) As String
            If userid.Length > 4 Then
                userid = userid.Substring(0, 4)
            End If

            Dim output As String = DirectCast(DoScalar("SELECT Email FROM UserList WHERE UserID = '" & userid & "'", GetGoatDBConnection()), String)
            If output IsNot Nothing Then
                Return output
            Else
                Return "Email for userid: " & userid & " not found<p/>"
            End If
        End Function

        Public Function GetMailingListInfoByEmailAddress(ByVal email As String) As DataTable
            Dim sql As String = "SELECT FirstName, LastName, Email FROM MailingList where Email = '" & email & "'"
            Dim result As DataTable = DoQuery(sql, GetGoatDBConnection())
            Return result
        End Function

        Public Function AddToMailingList(ByVal first As String, ByVal last As String, ByVal email As String) As String
            Dim sql As String = "insert into mailinglist (firstname, lastname, email) values ('" & first & "', '" & last & "', '" & email & "')"
            Dim result As String = DoNonQuery(sql, GetGoatDBConnection())
            Return result
        End Function

        Public Function GetAllPostings() As DataTable
            Dim sql As String = "SELECT Title, Email, Message FROM Postings"
            Dim result As DataTable = DoQuery(sql, GetGoatDBConnection())
            Return result
        End Function

        Public Function AddNewPosting(ByVal title As String, ByVal email As String, ByVal message As String) As String
            Dim sql As String = "insert into Postings(title, email, message) values ('" & title & "','" & email & "','" & message & "')"
            Dim result As String = DoNonQuery(sql, GetGoatDBConnection())
            Return result
        End Function

        Public Function GetPostingLinks() As DataTable
            Dim sql As String = "SELECT PostingID, Title FROM Postings"
            Dim result As DataTable = DoQuery(sql, GetGoatDBConnection())
            Return result
        End Function

        Public Function GetPostingByID(ByVal id As Integer) As DataTable
            Dim sql As String = "SELECT Title, Email, Message FROM Postings where PostingID=" & id
            Dim result As DataTable = DoQuery(sql, GetGoatDBConnection())
            Return result
        End Function

    End Class
End Namespace