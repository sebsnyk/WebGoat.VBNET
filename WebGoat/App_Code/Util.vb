Imports System
Imports System.Diagnostics
Imports log4net
Imports System.Reflection
Imports System.IO
Imports System.Threading

Namespace OWASP.WebGoat.NET.App_Code
    Public Class Util
        Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)
        
        Public Shared Function RunProcessWithInput(ByVal cmd As String, ByVal args As String, ByVal input As String) As Integer
            Dim startInfo As New ProcessStartInfo With {
                .WorkingDirectory = Settings.RootDir,
                .FileName = cmd,
                .Arguments = args,
                .UseShellExecute = False,
                .RedirectStandardInput = True,
                .RedirectStandardError = True,
                .RedirectStandardOutput = True,
                }

            Using process As New Process()
                process.EnableRaisingEvents = True
                process.StartInfo = startInfo

                AddHandler process.OutputDataReceived, Sub(sender, e)
                                                         If e.Data IsNot Nothing Then
                                                             log.Info(e.Data)
                                                         End If
                                                     End Sub

                AddHandler process.ErrorDataReceived, Sub(sender, e)
                                                        If e.Data IsNot Nothing Then
                                                            log.Error(e.Data)
                                                        End If
                                                    End Sub

                Dim are As New AutoResetEvent(False)

                AddHandler process.Exited, Sub(sender, e)
                                               Thread.Sleep(1000)
                                               are.Set()
                                               log.Info("Process exited")
                                           End Sub

                process.Start()

                Using reader As New StreamReader(New FileStream(input, FileMode.Open))
                    Dim line As String
                    Dim replaced As String
                    While (Function() line = reader.ReadLine(), line)() IsNot Nothing
                        If Environment.OSVersion.Platform = PlatformID.Win32NT Then
                            replaced = line.Replace("DB_Scripts/datafiles/", "DB_Scripts\\datafiles\\")
                        Else
                            replaced = line
                        End If

                        log.Debug("Line: " & replaced)

                        process.StandardInput.WriteLine(replaced)
                    End While
                End Using
    
                process.StandardInput.Close()
    

                process.BeginOutputReadLine()
                process.BeginErrorReadLine()
    
                'NOTE: Looks like we have a mono bug: https://bugzilla.xamarin.com/show_bug.cgi?id=6291
                'have a wait time for now.
                
                are.WaitOne(10 * 1000)

                If process.HasExited Then
                    Return process.ExitCode
                Else 'WTF? Should have exited dammit!
                    process.Kill()
                    Return 1
                End If
            End Using
        End Function
    End Class
End Namespace