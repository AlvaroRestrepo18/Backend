using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TechNova.API.Models;

namespace TechNova.API.Data;

public partial class TechNovaContext : DbContext
{
    public TechNovaContext()
    {
    }

    public TechNovaContext(DbContextOptions<TechNovaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<DetalleCompra> DetalleCompras { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Permisoxrol> Permisoxrols { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Productoxventum> Productoxventa { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<Servicioxventum> Servicioxventa { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=TechNova;Username=postgres;Password=admin123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categorias_pkey");

            entity.ToTable("categorias");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.TipoCategoria)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Producto'::character varying")
                .HasColumnName("tipo_categoria");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("clientes_pkey");

            entity.ToTable("clientes");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsRequired() // NOT NULL en la BD
                .HasColumnName("nombre");

            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .HasColumnName("apellido");

            entity.Property(e => e.TipoDoc)
                .HasMaxLength(10)
                .HasColumnName("tipo_doc");

            entity.Property(e => e.Documento)
                .HasColumnName("documento");

            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");

            entity.Property(e => e.Estado)
                .HasColumnName("estado");

            // Restricción UNIQUE (documento)
            entity.HasIndex(e => e.Documento, "clientes_documento_key").IsUnique();
        });


        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("compras_pkey");

            entity.ToTable("compras");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");

            entity.Property(e => e.FechaCompra)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_compra");

            entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");

            entity.Property(e => e.MetodoPago).HasColumnName("metodo_pago");

            entity.Property(e => e.Estado).HasColumnName("estado");

            entity.Property(e => e.Subtotal)
                .HasPrecision(10, 2)
                .HasColumnName("subtotal");

            entity.Property(e => e.Iva)
                .HasPrecision(10, 2)
                .HasColumnName("iva");

            entity.Property(e => e.Total)
                .HasPrecision(10, 2)
                .HasColumnName("total");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.Compras)
                .HasForeignKey(d => d.ProveedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("compras_proveedor_id_fkey");
        });

        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("detalle_compra_pkey");

            entity.ToTable("detalle_compra");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompraId).HasColumnName("compra_id");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario");
            entity.Property(e => e.SubtotalItems).HasColumnName("subtotal_items");

            entity.HasOne(d => d.Compra).WithMany(p => p.DetallesCompra)
                .HasForeignKey(d => d.CompraId);

        });



        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.IdPermiso).HasName("permiso_pkey");

            entity.ToTable("permiso");

            entity.Property(e => e.IdPermiso).HasColumnName("id_permiso");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Permisoxrol>(entity =>
        {
            entity.HasKey(e => e.IdPermisoRol).HasName("permisoxrol_pkey");

            entity.ToTable("permisoxrol");

            entity.Property(e => e.IdPermisoRol).HasColumnName("id_permiso_rol");
            entity.Property(e => e.FkPermiso).HasColumnName("fk_permiso");
            entity.Property(e => e.FkRol).HasColumnName("fk_rol");

            entity.HasOne(d => d.FkPermisoNavigation).WithMany(p => p.Permisoxrols)
                .HasForeignKey(d => d.FkPermiso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("permisoxrol_permiso_fk");

            entity.HasOne(d => d.FkRolNavigation).WithMany(p => p.Permisoxrols)
                .HasForeignKey(d => d.FkRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("permisoxrol_rol_fk");
        });

        // ✅ Producto corregido al script original
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productos_pkey");

            entity.ToTable("productos");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");

            entity.Property(e => e.Precio)
                .HasPrecision(10, 2)
                .HasColumnName("precio");

            entity.Property(e => e.Cantidad).HasColumnName("cantidad");

            // 👇 Aquí especificamos que es "date"
            entity.Property(e => e.FechaCreacion)
                .HasColumnType("date")
                .HasColumnName("fecha_creacion");

            entity.HasOne(d => d.Categoria)
                .WithMany(p => p.Productos)
                .HasForeignKey(d => d.CategoriaId)
                .HasConstraintName("productos_categoria_id_fkey");
        });
        modelBuilder.Entity<Productoxventum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productoxventa_pkey");

            entity.ToTable("productoxventa");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.VentaId).HasColumnName("venta_id");
            entity.Property(e => e.ValorUnitario)
                .HasPrecision(10, 2)
                .HasColumnName("valor_unitario");
            entity.Property(e => e.ValorTotal)
                .HasPrecision(10, 2)
                .HasColumnName("valor_total");

            entity.HasOne(d => d.Producto).WithMany(p => p.Productoxventa)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("productoxventa_producto_id_fkey");

            entity.HasOne(d => d.Venta).WithMany(p => p.Productoxventa)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("productoxventa_venta_id_fkey");
        });


        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proveedores_pkey");

            entity.ToTable("proveedores");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");

            entity.Property(e => e.Direccion).HasColumnName("direccion");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");

            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");

            entity.Property(e => e.TipoPersona)
                .HasMaxLength(20)
                .HasColumnName("tipo_persona");

            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(20)
                .HasColumnName("numero_documento");

            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(10)
                .HasColumnName("tipo_documento");

            entity.Property(e => e.Nombres)
                .HasMaxLength(50)
                .HasColumnName("nombres");

            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .HasColumnName("apellidos");

            entity.Property(e => e.RazonSocial)
                .HasMaxLength(100)
                .HasColumnName("razon_social");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .HasColumnName("nombre_rol");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("servicios_pkey");

            entity.ToTable("servicios");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.FkCategoria).HasColumnName("fk_categoria");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasPrecision(12, 2)
                .HasColumnName("precio");

            entity.HasOne(d => d.FkCategoriaNavigation).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.FkCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("servicios_categoria_fk");
        });

        modelBuilder.Entity<Servicioxventum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("servicioxventa_pkey");

            entity.ToTable("servicioxventa");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FkServicio).HasColumnName("fk_servicio");
            entity.Property(e => e.FkVenta).HasColumnName("fk_venta");
            entity.Property(e => e.Precio)
                .HasPrecision(12, 2)
                .HasColumnName("precio");

            entity.HasOne(d => d.FkServicioNavigation).WithMany(p => p.Servicioxventa)
                .HasForeignKey(d => d.FkServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("servicioxventa_servicio_fk");

            entity.HasOne(d => d.FkVentaNavigation).WithMany(p => p.Servicioxventa)
                .HasForeignKey(d => d.FkVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("servicioxventa_venta_fk");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");

            entity.HasKey(e => e.IdUsuario).HasName("usuarios_pkey");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.FkRol).HasColumnName("fk_rol");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.Contrasena).HasColumnName("contrasena").HasMaxLength(255);
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.TipoDoc).HasColumnName("tipo_doc").HasMaxLength(20);
            entity.Property(e => e.Documento).HasColumnName("documento").HasMaxLength(20);
            entity.Property(e => e.Celular).HasColumnName("celular").HasMaxLength(15);
            entity.Property(e => e.Direccion).HasColumnName("direccion").HasMaxLength(100);
            entity.Property(e => e.CodigoRecuperacion).HasColumnName("CodigoRecuperacion").HasMaxLength(10);
            entity.Property(e => e.CodigoExpira).HasColumnName("CodigoExpira");

            entity.HasOne(d => d.FkRolNavigation)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.FkRol)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("usuarios_rol_fk");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ventas_pkey");

            entity.ToTable("ventas");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha");
            entity.Property(e => e.FkCliente).HasColumnName("fk_cliente");
            entity.Property(e => e.Total)
                .HasPrecision(12, 2)
                .HasColumnName("total");

            entity.HasOne(d => d.FkClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.FkCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ventas_cliente_fk");
        });

        modelBuilder.HasSequence("categorias_id_seq");
        modelBuilder.HasSequence("clientes_id_seq");
        modelBuilder.HasSequence("compras_id_seq");
        modelBuilder.HasSequence("detalle_compra_id_seq");
        modelBuilder.HasSequence("permiso_id_permiso_seq");
        modelBuilder.HasSequence("permisoxrol_id_permiso_rol_seq");
        modelBuilder.HasSequence("productos_id_seq");
        modelBuilder.HasSequence("productoxventa_id_seq");
        modelBuilder.HasSequence("proveedores_id_seq");
        modelBuilder.HasSequence("roles_id_rol_seq");
        modelBuilder.HasSequence("servicios_id_seq");
        modelBuilder.HasSequence("servicioxventa_id_seq");
        modelBuilder.HasSequence("usuarios_id_usuario_seq");
        modelBuilder.HasSequence("ventas_id_seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
