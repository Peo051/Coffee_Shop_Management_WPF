-- Tạo cơ sở dữ liệu

IF DB_ID(N'CoffeeShopDb') IS NULL
BEGIN
    CREATE DATABASE CoffeeShopDb;
END
GO

