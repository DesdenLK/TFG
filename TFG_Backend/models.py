from sqlalchemy import Boolean, Column, Float, ForeignKey, Integer, LargeBinary, String, UUID, DateTime, Enum
from enum import Enum as PyEnum
from sqlalchemy.orm import relationship
from sqlalchemy.ext.declarative import declarative_base
import uuid
from datetime import datetime
import bcrypt

Base = declarative_base()

class Users(Base):
    __tablename__ = 'users'

    id = Column(Integer, primary_key=True, autoincrement=True)
    uuid = Column(UUID(as_uuid=True), unique=True, nullable=False, default=uuid.uuid4)
    name = Column(String(50), nullable=False, unique=True)
    password = Column(String, nullable=False)
    created_at = Column(DateTime, nullable=False, default=datetime.now)


    def set_password(self, password):
        hashed_bytes = bcrypt.hashpw(password.encode('utf-8'), bcrypt.gensalt())
        self.password = hashed_bytes.decode('utf-8')
    
    def check_password(self, password):
        return bcrypt.checkpw(password.encode('utf-8'), self.password.encode('utf-8'))

class FileType(PyEnum):
    Texture = "Texture"
    Heightmap = "Heightmap"

class FileStorage(Base):
    __tablename__ = 'file_storage'

    id = Column(Integer, primary_key=True, autoincrement=True)
    terrain_uuid = Column(UUID(as_uuid=True), ForeignKey('terrains.uuid'), nullable=False)
    uuid = Column(UUID(as_uuid=True), unique=True, nullable=False, default=uuid.uuid4)
    filename = Column(String(255), nullable=False, unique=True)
    filetype = Column(Enum(FileType), nullable=False)
    file_data = Column(LargeBinary, nullable=False)
    created_at = Column(DateTime, nullable=False, default=datetime.now)

class Terrains(Base):
    __tablename__ = 'terrains'
    id = Column(Integer, primary_key=True, autoincrement=True)
    uuid = Column(UUID(as_uuid=True), unique=True, nullable=False, default=uuid.uuid4)
    name = Column(String, nullable=False, unique=True)
    description = Column(String, nullable=False)
    isPublic = Column(Boolean, nullable=False, default=False)
    heightmapResolution = Column(Integer, nullable=False)
    widthmapResolution = Column(Integer, nullable=False)
    size_X = Column(Integer, nullable=False)
    size_Y = Column(Integer, nullable=False)
    size_Z = Column(Integer, nullable=False)
    created_at = Column(DateTime, nullable=False, default=datetime.now)

class TerrainLevels(Base):
    __tablename__ = 'terrain_levels'
    id = Column(Integer, primary_key=True, autoincrement=True)
    uuid = Column(UUID(as_uuid=True), unique=True, nullable=False, default=uuid.uuid4)
    name = Column(String, nullable=False, unique=True)
    description = Column(String, nullable=False)
    terrain_uuid = Column(UUID(as_uuid=True), ForeignKey('terrains.uuid'), nullable=False)
    start_X = Column(Float, nullable=False)
    start_Y = Column(Float, nullable=False)
    start_Z = Column(Float, nullable=False)
    end_X = Column(Float, nullable=False)
    end_Y = Column(Float, nullable=False)
    end_Z = Column(Float, nullable=False)
    creator = Column(UUID(as_uuid=True), ForeignKey('users.uuid'), nullable=False)
    created_at = Column(DateTime, nullable=False, default=datetime.now)