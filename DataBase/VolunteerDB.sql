CREATE DATABASE VolunteerManagement;
USE VolunteerManagement;

-- Bảng Users (Người dùng)
CREATE TABLE Users (
    user_id VARCHAR(50) PRIMARY KEY,
    user_name VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL
);

-- Bảng Volunteers (Tình nguyện viên)
CREATE TABLE Volunteers (
    volunteer_id VARCHAR(50) PRIMARY KEY,
    phone_number VARCHAR(50),
    email NVARCHAR(100),
    name NVARCHAR(100),
    date_of_birth DATETIME,
    gender NVARCHAR(10),
    image_path NVARCHAR(200),
    address NVARCHAR(100)
);

-- Bảng Admin (Quản trị viên)
CREATE TABLE Admin (
    admin_id VARCHAR(50) PRIMARY KEY,
    name NVARCHAR(100),
    img_path NVARCHAR(200),
    email NVARCHAR(100)
);

-- Bảng Organization (Nhà tổ chức tình nguyện)
CREATE TABLE Organization (
    org_id VARCHAR(50) PRIMARY KEY,
    name NVARCHAR(100),
    email VARCHAR(100),
    address NVARCHAR(100),
    phone_number VARCHAR(50),
    image_path NVARCHAR(50),
    description NVARCHAR(200)
);

CREATE TABLE Events (
    event_id VARCHAR(50) PRIMARY KEY,
    org_id VARCHAR(50),
    type_event_id VARCHAR(50),
    name NVARCHAR(100),
    description NVARCHAR(200),
    day_begin DATETIME,
    day_end DATETIME,
    location NVARCHAR(100),
    target_member INT,
    target_funds INT,
    image_path NVARCHAR(100),
    list_img NVARCHAR(100),
    status BIT DEFAULT 0, -- 0: Không được duyệt, 1: Được duyệt
    FOREIGN KEY (org_id) REFERENCES Organization(org_id)
);


-- Bảng Registrations (Đăng ký tham gia)
CREATE TABLE Registrations (
    reg_id VARCHAR(50) PRIMARY KEY,
    volunteer_id VARCHAR(50),
    event_id VARCHAR(50),
    status NVARCHAR(20) CHECK (status IN (N'Đã duyệt', N'Chờ duyệt', N'Hủy')), -- Chỉ cho phép 3 giá trị này
    FOREIGN KEY (volunteer_id) REFERENCES Volunteers(volunteer_id),
    FOREIGN KEY (event_id) REFERENCES Events(event_id)
);

-- Bảng Donation (Quyên góp)
CREATE TABLE Donation (
    donation_id VARCHAR(50) PRIMARY KEY,
    volunteer_id VARCHAR(50),
    event_id VARCHAR(50),
    amount MONEY,
    message NVARCHAR(500),
    donation_date DATE,
    FOREIGN KEY (volunteer_id) REFERENCES Volunteers(volunteer_id),
    FOREIGN KEY (event_id) REFERENCES Events(event_id)
);

-- Bảng Event_Types (Loại sự kiện)
CREATE TABLE Event_Types (
    type_id VARCHAR(50) PRIMARY KEY, -- Khóa chính kiểu VARCHAR(50)
    type_name VARCHAR(255) NOT NULL
);
