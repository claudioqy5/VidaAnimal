CREATE TABLE Ventas (
    VentaID int IDENTITY(1,1) PRIMARY KEY,
    ClienteID int NULL,
    UsuarioID int NULL,
    SerieComprobante varchar(50) NOT NULL,
    NumeroComprobante varchar(50) NOT NULL,
    FechaVenta datetime2 NOT NULL,
    TotalVenta decimal(12,2) NOT NULL
);

CREATE TABLE VentaDetalles (
    VentaDetalleID int IDENTITY(1,1) PRIMARY KEY,
    VentaID int NOT NULL,
    ProductoID int NOT NULL,
    Cantidad decimal(12,2) NOT NULL,
    PrecioVentaUnitario decimal(12,2) NOT NULL,
    SubTotal decimal(12,2) NOT NULL,
    CONSTRAINT FK_VentaDetalles_Ventas FOREIGN KEY (VentaID) REFERENCES Ventas(VentaID) ON DELETE CASCADE,
    CONSTRAINT FK_VentaDetalles_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID) ON DELETE NO ACTION
);
