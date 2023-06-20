Imports System

Namespace OWASP.WebGoat.NET.App_Code
    Public Class VeryWeakRandom
        Private _seed As UInteger = 7
        Private _helper As UInteger = 1

        Public Sub New()
        End Sub

        Public Sub New(seed As UInteger)
            _seed = seed
        End Sub

        Public Function Next(min As UInteger, max As UInteger) As UInteger
            _seed = Peek(min, max)
            _helper += 1

            Return _seed
        End Function

        Public Function Peek(min As UInteger, max As UInteger) As UInteger
            If min >= max Then
                Throw New Exception("Min must be smaller than max")
            End If

            Dim seed = _seed + _helper

            If seed > max Then
                seed = min
            End If

            Return seed
        End Function
    End Class
End Namespace