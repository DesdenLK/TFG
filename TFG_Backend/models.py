from sqlalchemy import Boolean, Column, Float, ForeignKey, Index, Integer, LargeBinary, String, UUID, DateTime, Enum
from enum import Enum as PyEnum
from sqlalchemy.orm import relationship
from sqlalchemy.ext.declarative import declarative_base
import uuid
from datetime import datetime
import bcrypt

Base = declarative_base()

# Defineix la taula d'usuaris
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

# Defineix un enumerat per als tipus de fitxers
class FileType(PyEnum):
    Texture = "Texture"
    Heightmap = "Heightmap"
    Avalanche = "Avalanche"

# Defineix la taula de fitxers
class FileStorage(Base):
    __tablename__ = 'file_storage'

    id = Column(Integer, primary_key=True, autoincrement=True)
    terrain_uuid = Column(UUID(as_uuid=True), ForeignKey('terrains.uuid'), nullable=False)
    uuid = Column(UUID(as_uuid=True), unique=True, nullable=False, default=uuid.uuid4)
    filename = Column(String(255), nullable=False)
    filetype = Column(Enum(FileType), nullable=False)
    file_data = Column(LargeBinary, nullable=False)
    created_at = Column(DateTime, nullable=False, default=datetime.now)

# Defineix la taula de terrenys
class Terrains(Base):
    __tablename__ = 'terrains'
    id = Column(Integer, primary_key=True, autoincrement=True)
    uuid = Column(UUID(as_uuid=True), unique=True, nullable=False, default=uuid.uuid4)
    name = Column(String, nullable=False)
    description = Column(String, nullable=False)
    isPublic = Column(Boolean, nullable=False, default=False)
    heightmapResolution = Column(Integer, nullable=False)
    widthmapResolution = Column(Integer, nullable=False)
    size_X = Column(Integer, nullable=False)
    size_Y = Column(Integer, nullable=False)
    size_Z = Column(Integer, nullable=False)
    creator = Column(UUID(as_uuid=True), ForeignKey('users.uuid'), nullable=False)
    created_at = Column(DateTime, nullable=False, default=datetime.now)

# Defineix la taula de nivells de terreny
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
    optimal_total3D_distance = Column(Float, nullable=False, default=0.0)
    optimal_total2D_distance = Column(Float, nullable=False, default=0.0)
    optimal_total_slope = Column(Float, nullable=False, default=0.0)
    optimal_total_positive_slope = Column(Float, nullable=False, default=0.0)
    optimal_total_negative_slope = Column(Float, nullable=False, default=0.0)
    optimal_metabolic_cost = Column(Float, nullable=False, default=0.0)
    optimal_total_avalanches = Column(Integer, nullable=False, default=0)
    created_at = Column(DateTime, nullable=False, default=datetime.now)

# Defineix la taula de puntuacions dels nivells
class LevelScores(Base):
    __tablename__ = 'level_scores'
    id = Column(Integer, primary_key=True, autoincrement=True)
    uuid = Column(UUID(as_uuid=True), unique=True, nullable=False, default=uuid.uuid4)
    level_uuid = Column(UUID(as_uuid=True), ForeignKey('terrain_levels.uuid'), nullable=False)
    user_uuid = Column(UUID(as_uuid=True), ForeignKey('users.uuid'), nullable=False)
    score = Column(Integer, nullable=False, default=0.0)
    total3D_distance = Column(Float, nullable=False)
    total2D_distance = Column(Float, nullable=False)
    total_slope = Column(Float, nullable=False)
    total_positive_slope = Column(Float, nullable=False)
    total_negative_slope = Column(Float, nullable=False)
    metabolic_cost = Column(Float, nullable=False)
    total_avalanches = Column(Integer, nullable=False, default=0)
    created_at = Column(DateTime, nullable=False, default=datetime.now)


# Defineix la taula de punts del camí òptim
class OptimalPathPoint(Base):
    __tablename__ = 'optimal_path_points'
    id = Column(Integer, primary_key=True, autoincrement=True)
    uuid = Column(UUID(as_uuid=True), unique=True, nullable=False, default=uuid.uuid4)
    level_uuid = Column(UUID(as_uuid=True), ForeignKey('terrain_levels.uuid'), nullable=False)
    index = Column(Integer, nullable=False) # Index of the point in the path
    point_X = Column(Float, nullable=False)
    point_Y = Column(Float, nullable=False)
    point_Z = Column(Float, nullable=False)
    created_at = Column(DateTime, nullable=False, default=datetime.now)
    __table_args__ = (
        Index('idx_level_uuid_index', 'level_uuid', 'index'),
    )