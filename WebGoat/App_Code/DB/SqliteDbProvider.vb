Imports System
Imports System.Data
Imports System.IO
Imports System.Reflection
Imports log4net
Imports Mono.Data.Sqlite

Namespace OWASP.WebGoat.NET.App_Code.DB
    Public Class SqliteDbProvider
        Inherits IDbProvider

        Private _connectionString As String = String.Empty
        Private _clientExec As String
        Private _dbFileName As String
        Private log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

        Public ReadOnly Property Name As String
            Get
                Return DbConstants.DB_TYPE_SQLITE
            End Get
        End Property

        Public Sub New(ByVal configFile As ConfigFile)
            _connectionString = String.Format("Data Source={0};Version=3", configFile.[Get](DbConstants.KEY_FILE_NAME))
            _clientExec = configFile.[Get](DbConstants.KEY_CLIENT_EXEC)
            _dbFileName = configFile.[Get](DbConstants.KEY_FILE_NAME)
            If Not File.Exists(_dbFileName) Then SqliteConnection.CreateFile(_dbFileName)
        End Sub

        Public Function TestConnection() As Boolean
            Try

                Using conn As SqliteConnection = New SqliteConnection(_connectionString)
                    conn.Open()

                    Using cmd As SqliteCommand = conn.CreateCommand()
                        cmd.CommandText = "SELECT date('now')"
                        cmd.CommandType = CommandType.Text
                        cmd.ExecuteReader()
                    End Using
                End Using

                Return True
            Catch ex As Exception
                log.[Error]("Error testing DB", ex)
                Return False
            End Try
        End Function

        Public Function GetCatalogData() As DataSet
            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim da As SqliteDataAdapter = New SqliteDataAdapter("select * from Products", connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)
                Return ds
            End Using
        End Function

        Public Function IsValidCustomerLogin(ByVal email As String, ByVal password As String) As Boolean
            Dim encoded_password As String = Encoder.Encode(password)
            Dim sql As String = "select * from CustomerLogin where email = '" & email & "' and password = '" & encoded_password & "';"

            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                Try
                    Return ds.Tables(0).Rows.Count <> 0
                Catch ex As Exception
                    log.[Error]("Error checking login", ex)
                    Throw New Exception("Error checking login", ex)
                End Try
            End Using
        End Function

        Public Function IsAdminCustomerLogin(ByVal email As String) As Boolean
            Using connection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim command = New SqliteCommand("SELECT is_admin FROM CustomerLogin where email = @email", connection)
                command.Parameters.AddWithValue("@email", email)
                Dim result = CType(command.ExecuteScalar(), Long?)
                Return result.HasValue AndAlso result.Value = 1
            End Using
        End Function

        Public Function CreateCustomer(ByVal name As String, ByVal email As String, ByVal password As String, ByVal isAdmin As Boolean, ByVal question As Integer, ByVal answer As String) As Boolean
            Using connection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim da = New SqliteDataAdapter("SELECT email FROM CustomerLogin WHERE email = '" & email & "'", connection)
                Dim ds = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count <> 0 Then
                    Return False
                End If

                Dim insertCustomerCommand = New SqliteCommand("INSERT INTO Customers " & "(customerName, logoFileName, contactLastName, contactFirstName, phone, addressLine1, addressLine2, city, state, postalCode, country, salesRepEmployeeNumber, creditLimit) " & "VALUES (@name, '', '', '', '', '', '', '', '', '', '', '', '')", connection)
                insertCustomerCommand.Parameters.AddWithValue("@name", name)
                insertCustomerCommand.ExecuteNonQuery()
                Dim lastInsertRowidCommand = New SqliteCommand("SELECT last_insert_rowid()", connection)
                Dim id = CLng(lastInsertRowidCommand.ExecuteScalar())
                Dim insertCustomerLogin = New SqliteCommand("INSERT INTO CustomerLogin " & "(email, customerNumber, password, question_id, answer, is_admin) " & "VALUES (@email, @id, @password, @question, @answer, @is_admin)", connection)
                insertCustomerLogin.Parameters.AddWithValue("@email", email)
                insertCustomerLogin.Parameters.AddWithValue("@id", id)
                insertCustomerLogin.Parameters.AddWithValue("@password", Encoder.Encode(password))
                insertCustomerLogin.Parameters.AddWithValue("@question", question)
                insertCustomerLogin.Parameters.AddWithValue("@answer", answer)
                insertCustomerLogin.Parameters.AddWithValue("@is_admin", If(isAdmin, 1, 0))
                insertCustomerLogin.ExecuteNonQuery()
                Return True
            End Using
        End Function

        Public Function RecreateGoatDb() As Boolean
            Try
                log.Info("Running recreate")
                Dim args As String = String.Format("""{0}""", _dbFileName)
                Dim script As String = Path.Combine(Settings.RootDir, DbConstants.DB_CREATE_SQLITE_SCRIPT)
                Dim retVal1 As Integer = Math.Abs(Util.RunProcessWithInput(_clientExec, args, script))
                script = Path.Combine(Settings.RootDir, DbConstants.DB_LOAD_SQLITE_SCRIPT)
                Dim retVal2 As Integer = Math.Abs(Util.RunProcessWithInput(_clientExec, args, script))
                Return Math.Abs(retVal1) + Math.Abs(retVal2) = 0
            Catch ex As Exception
                log.[Error]("Error rebulding DB", ex)
                Return False
            End Try
        End Function

        Public Function CustomCustomerLogin(ByVal email As String, ByVal password As String) As String
            Dim error_message As String = Nothing

            Try
                Dim sql As String = "select * from CustomerLogin where email = '" & email & "';"

                Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                    connection.Open()
                    Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                    Dim ds As DataSet = New DataSet()
                    da.Fill(ds)

                    If ds.Tables(0).Rows.Count = 0 Then
                        error_message = "Email Address Not Found!"
                        Return error_message
                    End If

                    Dim encoded_password As String = ds.Tables(0).Rows(0)("Password").ToString()
                    Dim decoded_password As String = Encoder.Decode(encoded_password)

                    If password.Trim().ToLower() <> decoded_password.Trim().ToLower() Then
                        error_message = "Password Not Valid For This Email Address!"
                    Else
                        error_message = Nothing
                    End If
                End Using

            Catch ex As SqliteException
                log.[Error]("Error with custom customer login", ex)
                error_message = ex.Message
            Catch ex As Exception
                log.[Error]("Error with custom customer login", ex)
            End Try

            Return error_message
        End Function

        Public Function GetCustomerEmail(ByVal customerNumber As String) As String
            Dim output As String = Nothing

            Try

                Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                    connection.Open()
                    Dim sql As String = "select email from CustomerLogin where customerNumber = " & customerNumber
                    Dim command As SqliteCommand = New SqliteCommand(sql, connection)
                    output = command.ExecuteScalar().ToString()
                End Using

            Catch ex As Exception
                output = ex.Message
            End Try

            Return output
        End Function

        Public Function GetCustomerDetails(ByVal customerNumber As String) As DataSet
            Dim sql As String = "select Customers.customerNumber, Customers.customerName, Customers.logoFileName, Customers.contactLastName, Customers.contactFirstName, " & "Customers.phone, Customers.addressLine1, Customers.addressLine2, Customers.city, Customers.state, Customers.postalCode, Customers.country, " & "Customers.salesRepEmployeeNumber, Customers.creditLimit, CustomerLogin.email, CustomerLogin.password, CustomerLogin.question_id, CustomerLogin.answer " & "From Customers, CustomerLogin where Customers.customerNumber = CustomerLogin.customerNumber and Customers.customerNumber = " & customerNumber
            Dim ds As DataSet = New DataSet()

            Try

                Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                    connection.Open()
                    Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                    da.Fill(ds)
                End Using

            Catch ex As Exception
                log.[Error]("Error getting customer details", ex)
                Throw New ApplicationException("Error getting customer details", ex)
            End Try

            Return ds
        End Function

        Public Function GetOffice(ByVal city As String) As DataSet
            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim sql As String = "select * from Offices where city = @city"
                Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                da.SelectCommand.Parameters.AddWithValue("@city", city)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)
                Return ds
            End Using
        End Function

        Public Function GetMessages(ByVal customerLogin As String) As DataSet
            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim sql As String = "SELECT Messages.id, Messages.title, Messages.text " & "FROM   Messages " & "INNER JOIN CustomerLogin ON CustomerLogin.customerNumber = Messages.customerId " & "WHERE (CustomerLogin.email = @login)"
                Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                da.SelectCommand.Parameters.AddWithValue("@login", customerLogin)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)
                Return ds
            End Using
        End Function

        Public Function GetComments(ByVal productCode As String) As DataSet
            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim sql As String = "select * from Comments where productCode = @productCode"
                Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                da.SelectCommand.Parameters.AddWithValue("@productCode", productCode)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)
                Return ds
            End Using
        End Function

        Public Function AddComment(ByVal productCode As String, ByVal email As String, ByVal comment As String) As String
            Dim sql As String = "insert into Comments(productCode, email, comment) values ('" & productCode & "','" & email & "','" & comment & "');"
            Dim output As String = Nothing

            Try

                Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                    connection.Open()
                    Dim command As SqliteCommand = New SqliteCommand(sql, connection)
                    command.ExecuteNonQuery()
                End Using

            Catch ex As Exception
                log.[Error]("Error adding comment", ex)
                output = ex.Message
            End Try

            Return output
        End Function

        Public Function UpdateCustomerPassword(ByVal customerNumber As Integer, ByVal password As String) As String
            Dim sql As String = "update CustomerLogin set password = '" & Encoder.Encode(password) & "' where customerNumber = " + customerNumber
            Dim output As String = Nothing

            Try

                Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                    connection.Open()
                    Dim command As SqliteCommand = New SqliteCommand(sql, connection)
                    Dim rows_added As Integer = command.ExecuteNonQuery()
                    log.Info("Rows Added: " & rows_added & " to comment table")
                End Using

            Catch ex As Exception
                log.[Error]("Error updating customer password", ex)
                output = ex.Message
            End Try

            Return output
        End Function

        Public Function GetSecurityQuestionAndAnswer(ByVal email As String) As String()
            Dim sql As String = "select SecurityQuestions.question_text, CustomerLogin.answer from CustomerLogin, " & "SecurityQuestions where CustomerLogin.email = '" & email & "' and CustomerLogin.question_id = " & "SecurityQuestions.question_id;"
            Dim qAndA As String() = New String(1) {}

            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count > 0 Then
                    Dim row As DataRow = ds.Tables(0).Rows(0)
                    qAndA(0) = row(0).ToString()
                    qAndA(1) = row(1).ToString()
                End If
            End Using

            Return qAndA
        End Function

        Public Function GetPasswordByEmail(ByVal email As String) As String
            Dim result As String = String.Empty

            Try

                Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                    connection.Open()
                    Dim sql As String = "select * from CustomerLogin where email = '" & email & "';"
                    Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                    Dim ds As DataSet = New DataSet()
                    da.Fill(ds)

                    If ds.Tables(0).Rows.Count = 0 Then
                        result = "Email Address Not Found!"
                    End If

                    Dim encoded_password As String = ds.Tables(0).Rows(0)("Password").ToString()
                    Dim decoded_password As String = Encoder.Decode(encoded_password)
                    result = decoded_password
                End Using

            Catch ex As Exception
                result = ex.Message
            End Try

            Return result
        End Function

        Public Function GetUsers() As DataSet
            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim sql As String = "select * from CustomerLogin;"
                Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)
                Return ds
            End Using
        End Function

        Public Function GetOrders(ByVal customerID As Integer) As DataSet
            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim sql As String = "select * from Orders where customerNumber = " & customerID
                Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count = 0 Then
                    Return Nothing
                Else
                    Return ds
                End If
            End Using
        End Function

        Public Function GetProductDetails(ByVal productCode As String) As DataSet
            Dim sql As String = String.Empty
            Dim da As SqliteDataAdapter
            Dim ds As DataSet = New DataSet()

            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                sql = "select * from Products where productCode = '" & productCode & "'"
                da = New SqliteDataAdapter(sql, connection)
                da.Fill(ds, "products")
                sql = "select * from Comments where productCode = '" & productCode & "'"
                da = New SqliteDataAdapter(sql, connection)
                da.Fill(ds, "comments")
                Dim dr As DataRelation = New DataRelation("prod_comments", ds.Tables("products").Columns("productCode"), ds.Tables("comments").Columns("productCode"), False)
                ds.Relations.Add(dr)
                Return ds
            End Using
        End Function

        Public Function GetOrderDetails(ByVal orderNumber As Integer) As DataSet
            Dim sql As String = "select Customers.customerName, Orders.customerNumber, Orders.orderNumber, Products.productName, " & "OrderDetails.quantityOrdered, OrderDetails.priceEach, Products.productImage " & "from OrderDetails, Products, Orders, Customers where " & "Customers.customerNumber = Orders.customerNumber " & "and OrderDetails.productCode = Products.productCode " & "and Orders.orderNumber = OrderDetails.orderNumber " & "and OrderDetails.orderNumber = " & orderNumber

            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count = 0 Then
                    Return Nothing
                Else
                    Return ds
                End If
            End Using
        End Function

        Public Function GetPayments(ByVal customerNumber As Integer) As DataSet
            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim sql As String = "select * from Payments where customerNumber = " & customerNumber
                Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count = 0 Then
                    Return Nothing
                Else
                    Return ds
                End If
            End Using
        End Function

        Public Function GetProductsAndCategories() As DataSet
            Return GetProductsAndCategories(0)
        End Function

        Public Function GetProductsAndCategories(ByVal catNumber As Integer) As DataSet
            Dim sql As String = String.Empty
            Dim da As SqliteDataAdapter
            Dim ds As DataSet = New DataSet()
            Dim catClause As String = String.Empty
            If catNumber >= 1 Then catClause += " where catNumber = " & catNumber

            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                sql = "select * from Categories" & catClause
                da = New SqliteDataAdapter(sql, connection)
                da.Fill(ds, "categories")
                sql = "select * from Products" & catClause
                da = New SqliteDataAdapter(sql, connection)
                da.Fill(ds, "products")
                Dim dr As DataRelation = New DataRelation("cat_prods", ds.Tables("categories").Columns("catNumber"), ds.Tables("products").Columns("catNumber"), False)
                ds.Relations.Add(dr)
                Return ds
            End Using
        End Function

        Public Function GetEmailByName(ByVal name As String) As DataSet
            Dim sql As String = "select firstName, lastName, email from Employees where firstName like '" & name & "%' or lastName like '" & name & "%'"

            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count = 0 Then
                    Return Nothing
                Else
                    Return ds
                End If
            End Using
        End Function

        Public Function GetEmailByCustomerNumber(ByVal num As String) As String
            Dim output As String = ""

            Try

                Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                    connection.Open()
                    Dim sql As String = "select email from CustomerLogin where customerNumber = " & num
                    Dim cmd As SqliteCommand = New SqliteCommand(sql, connection)
                    output = CStr(cmd.ExecuteScalar())
                End Using

            Catch ex As Exception
                log.[Error]("Error getting email by customer number", ex)
                output = ex.Message
            End Try

            Return output
        End Function

        Public Function GetCustomerEmails(ByVal email As String) As DataSet
            Dim sql As String = "select email from CustomerLogin where email like '" & email & "%'"

            Using connection As SqliteConnection = New SqliteConnection(_connectionString)
                connection.Open()
                Dim da As SqliteDataAdapter = New SqliteDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count = 0 Then
                    Return Nothing
                Else
                    Return ds
                End If
            End Using
        End Function
    End Class
End Namespace
