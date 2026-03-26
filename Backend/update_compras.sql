DROP TABLE IF EXISTS DetalleCompras;
DROP TABLE IF EXISTS CompraDetalles;
DROP TABLE IF EXISTS Compras;

CREATE TABLE Compras (
    CompraID INT IDENTITY(1,1) PRIMARY KEY,
    ProveedorID INT NOT NULL,
    UsuarioID INT NULL,
    NumeroComprobante NVARCHAR(50) NOT NULL,
    FechaCompra DATETIME2 NOT NULL DEFAULT GETDATE(),
    Total DECIMAL(12,2) NOT NULL,
    CONSTRAINT FK_Compras_Proveedores FOREIGN KEY (ProveedorID) REFERENCES Proveedores(ProveedorID),
    CONSTRAINT FK_Compras_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID)
);

CREATE TABLE CompraDetalles (
    CompraDetalleID INT IDENTITY(1,1) PRIMARY KEY,
    CompraID INT NOT NULL,
    ProductoID INT NOT NULL,
    Cantidad DECIMAL(12,3) NOT NULL,
    PrecioCostoUnitario DECIMAL(12,2) NOT NULL,
    SubTotal DECIMAL(12,2) NOT NULL,
    CONSTRAINT FK_CompraDetalles_Compras FOREIGN KEY (CompraID) REFERENCES Compras(CompraID) ON DELETE CASCADE,
    CONSTRAINT FK_CompraDetalles_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
);
