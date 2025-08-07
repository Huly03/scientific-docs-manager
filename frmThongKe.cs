using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
namespace SciDoc_Mgmt
{
    public partial class frmThongKe : Form
    {
       
        public frmThongKe()
        {
            InitializeComponent();
            LoadDataToComboBox();
            dropdownLoaiThongKe();
        }
        string connectionString = "Data Source=.;Initial Catalog=SciDoc_Mgmt;Integrated Security=True;";
        private void dropdownLoaiThongKe()
        {
            // Thêm các mục vào ComboBox
            cbxLoaiThongKe.Items.Add("Loại tài liệu");
            cbxLoaiThongKe.Items.Add("Nhà khoa học");
        }
            
        private void LoadDataToComboBox()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Load dữ liệu vào cbxLoaiTaiLieu
                    if (cbxLoaiTaiLieu != null)
                    {
                        string queryLoaiTaiLieu = "SELECT DISTINCT LoaiTaiLieu FROM TAILIEU";
                        DataTable dataTableLoaiTaiLieu = new DataTable();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(queryLoaiTaiLieu, connection))
                        {
                            adapter.Fill(dataTableLoaiTaiLieu);
                        }

                        if (dataTableLoaiTaiLieu.Rows.Count > 0)
                        {
                            cbxLoaiTaiLieu.DataSource = dataTableLoaiTaiLieu;
                            cbxLoaiTaiLieu.DisplayMember = "LoaiTaiLieu";
                            cbxLoaiTaiLieu.ValueMember = "LoaiTaiLieu";
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy dữ liệu loại tài liệu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    // Load dữ liệu vào cbxNhaKhoaHoc
                    if (cbxNhaKhoaHoc != null)
                    {
                        string queryNhaKhoaHoc = "SELECT TenNhaKhoaHoc FROM NHAKHOAHOC";
                        DataTable dataTableNhaKhoaHoc = new DataTable();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(queryNhaKhoaHoc, connection))
                        {
                            adapter.Fill(dataTableNhaKhoaHoc);
                        }

                        if (dataTableNhaKhoaHoc.Rows.Count > 0)
                        {
                            cbxNhaKhoaHoc.DataSource = dataTableNhaKhoaHoc;
                            cbxNhaKhoaHoc.DisplayMember = "TenNhaKhoaHoc";
                            cbxNhaKhoaHoc.ValueMember = "TenNhaKhoaHoc";
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy dữ liệu nhà khoa học.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi load dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            }

        private void btnThongKe_Click(object sender, EventArgs e)
        {
            // Chuẩn bị câu truy vấn
            string loaiThongKe = cbxLoaiThongKe.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(loaiThongKe))
            {
                MessageBox.Show("Vui lòng chọn loại thống kê!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string query = "";
            if (loaiThongKe == "Loại tài liệu")
            {
                query = @"
                SELECT 
                    LoaiTaiLieu AS [Loại Tài Liệu], 
                    COUNT(*) AS [Số Lượng Tài Liệu], 
                    MoTa AS [Mô Tả], 
                    NamXuatBan AS [Năm Xuất Bản], 
                    ChuyenNganh AS [Chuyên Ngành]
                FROM 
                    TAILIEU
                WHERE 
                    LoaiTaiLieu = @LoaiTaiLieu
                GROUP BY 
                    LoaiTaiLieu, MoTa, NamXuatBan, ChuyenNganh
                ";
                    }
                    else if (loaiThongKe == "Nhà khoa học")
                    {
                        query = @"
                SELECT 
                    NKH.TenNhaKhoaHoc AS [Nhà Khoa Học], 
                    COUNT(*) AS [Số Bài Đăng], 
                    NKH.HocVi AS [Học Vị], 
                    NKH.LinhVuc AS [Lĩnh Vực], 
                    T.LoaiTaiLieu AS [Loại Tài Liệu],
	                T.NamXuatBan AS [Năm Xuất Bản],
                    T.ChuyenNganh AS [Chuyên Ngành], 
                    VT.TenVaiTro AS [Vai Trò]
                FROM 
                    CHITIETTAILIEU CTT
                INNER JOIN 
                    NHAKHOAHOC NKH ON CTT.ID_NKH = NKH.ID_NKH
                INNER JOIN 
                    TAILIEU T ON CTT.ID_TaiLieu = T.ID_TaiLieu
                INNER JOIN 
                    VAITRO VT ON CTT.ID_VaiTro = VT.ID_VaiTro
                WHERE 
                    NKH.TenNhaKhoaHoc = @TenNhaKhoaHoc
                GROUP BY 
                    NKH.TenNhaKhoaHoc, 
                    NKH.HocVi, 
                    NKH.LinhVuc, 
                    T.LoaiTaiLieu, 
	                T.NamXuatBan,
                    T.ChuyenNganh, 
                    VT.TenVaiTro;

";
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    DataTable dataTable = new DataTable();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Gán tham số
                        if (loaiThongKe == "Loại tài liệu")
                            command.Parameters.AddWithValue("@LoaiTaiLieu", cbxLoaiTaiLieu.SelectedValue);
                        else if (loaiThongKe == "Nhà khoa học")
                            command.Parameters.AddWithValue("@TenNhaKhoaHoc", cbxNhaKhoaHoc.SelectedValue);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }

                    // Gán dữ liệu vào DataGridView
                    if (dataTable.Rows.Count == 0)
                    {
                        MessageBox.Show("Không tìm thấy dữ liệu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        dgvThongKe.DataSource = dataTable;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi thực hiện thống kê: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cbxLoaiTaiLieu_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cbxLoaiTaiLieu_DropDown(object sender, EventArgs e)
        {

        }

        

       
        private void dgvThongKe_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
