import os
from logging.config import fileConfig

from sqlalchemy import engine_from_config, pool
from alembic import context
from dotenv import load_dotenv

# Cargar variables desde el archivo .env
load_dotenv()

# Alembic Config
config = context.config

# Configurar logging si hay archivo .ini
if config.config_file_name:
    fileConfig(config.config_file_name)

# Importar metadatos desde tus modelos
from models import Base
target_metadata = Base.metadata

# Obtener la URL de la base de datos desde variable de entorno
database_url = os.getenv("DATABASE_URL")
if not database_url:
    raise Exception("DATABASE_URL no está definida en el entorno")

# Establecer la URL para Alembic
config.set_main_option("sqlalchemy.url", database_url)

def run_migrations_offline() -> None:
    """Migraciones sin conexión (genera SQLs, no ejecuta)."""
    context.configure(
        url=database_url,
        target_metadata=target_metadata,
        literal_binds=True,
        dialect_opts={"paramstyle": "named"},
    )

    with context.begin_transaction():
        context.run_migrations()


def run_migrations_online() -> None:
    """Migraciones con conexión (aplica cambios en la base de datos)."""
    connectable = engine_from_config(
        config.get_section(config.config_ini_section, {}),
        prefix="sqlalchemy.",
        poolclass=pool.NullPool,
    )

    with connectable.connect() as connection:
        context.configure(
            connection=connection,
            target_metadata=target_metadata
        )

        with context.begin_transaction():
            context.run_migrations()


# Ejecutar la migración adecuada según el modo
if context.is_offline_mode():
    run_migrations_offline()
else:
    run_migrations_online()
