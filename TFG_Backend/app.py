from typing import List
from fastapi import FastAPI, HTTPException, Request, Depends
from sqlalchemy import create_engine, or_
import sqlalchemy
from sqlalchemy.orm import sessionmaker, Session
from models import Base, Users  # Asegúrate de importar tus modelos desde tu archivo models.py
from dotenv import load_dotenv
import os
from pydantic import BaseModel

load_dotenv()
DATABASE_URL = os.getenv("DATABASE_URL")

app = FastAPI()
engine = create_engine(DATABASE_URL)
Session = sessionmaker(bind=engine)
Base.metadata.create_all(engine)

# Dependency to get the database session
def get_db():
    db = Session()
    try:
        yield db
    finally:
        db.close()


class User(BaseModel):
    name: str
    password: str

class TextureFile(BaseModel):
    textureFileName: str
    textureFileBytes: bytes

class Terrain(BaseModel):
    name: str
    description: str
    isPublic: bool
    heightmapResolution: int
    widthmapResolution: int
    size_X: int
    size_Y: int
    size_Z: int
    creator: str
    rawFileName: str
    rawFileBytes: bytes
    textureFiles: List[TextureFile]

class TerrainLevel(BaseModel):
    name: str
    description: str
    start_X: float
    start_Y: float
    start_Z : float
    end_X: float
    end_Y: float
    end_Z: float
    creator: str

class LevelScore(BaseModel):
    level_uuid: str
    user: str
    total2D_distance: float
    total3D_distance: float
    total_slope: float
    total_positive_slope: float
    total_negative_slope: float
    metabolic_cost: float


@app.post("/register-user")
async def register_user(user: User, db: sqlalchemy.orm.Session = Depends(get_db)):
    existing_user = db.query(Users).filter(Users.name == user.name).first()
    if existing_user:
        raise HTTPException(status_code=400, detail="Username already registered")


    new_user = Users(name=user.name)
    new_user.set_password(user.password)  

    try:
        db.add(new_user)
        db.commit()
        db.refresh(new_user)
        return {"message": "User registered successfully" , "statuscode": 200, "user": new_user.name}
    except Exception as e:
        db.rollback()
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        db.close()

@app.post("/login")
async def login(user: User, db: sqlalchemy.orm.Session = Depends(get_db)):
    db_user = db.query(Users).filter(Users.name == user.name).first()
    if db_user and db_user.check_password(user.password):  # Verifica si la contraseña es correcta
        return {"message": "Login successful", "statuscode": 200, "user": db_user.name}
    else:
        raise HTTPException(status_code=401, detail="Invalid credentials")

@app.post("/new-terrain")
async def new_terrain(terrain: Terrain, db: sqlalchemy.orm.Session = Depends(get_db)):
    from models import Terrains as TerrainModel
    from models import FileStorage as FileStorageModel

    new_terrain = TerrainModel(
        name=terrain.name,
        description=terrain.description,
        isPublic=terrain.isPublic,
        heightmapResolution=terrain.heightmapResolution,
        widthmapResolution=terrain.widthmapResolution,
        size_X=terrain.size_X,
        size_Y=terrain.size_Y,
        size_Z=terrain.size_Z
    )
    user = db.query(Users).filter(Users.name == terrain.creator).first()
    if not user:
        raise HTTPException(status_code=400, detail="User not found")
    new_terrain.creator = user.uuid

    try:
        db.add(new_terrain)
        db.flush()  
        db.refresh(new_terrain)

        new_raw_file = FileStorageModel(
            terrain_uuid=new_terrain.uuid,
            filename=terrain.rawFileName,
            filetype="Heightmap",
            file_data=bytes(terrain.rawFileBytes)
        )

        db.add(new_raw_file)
        for texture in terrain.textureFiles:
            new_texture_file = FileStorageModel(
                terrain_uuid=new_terrain.uuid,
                filename=texture.textureFileName,
                filetype="Texture",
                file_data=bytes(texture.textureFileBytes)
            )
            db.add(new_texture_file)
        db.commit()

        return {
            "message": "Terrain created successfully",
            "statuscode": 200,
            "terrain": {
                "name": new_terrain.name,
                "uuid": new_terrain.uuid
            }
        }
    except Exception as e:
        db.rollback()
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        db.close()

@app.get("/terrains/{username}")
async def get_terrains(username: str, db: sqlalchemy.orm.Session = Depends(get_db)):
    from models import Terrains as TerrainModel
    
    user = db.query(Users).filter(Users.name == username).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")

    terrains = db.query(TerrainModel).filter(
        or_(TerrainModel.creator == user.uuid, TerrainModel.isPublic == True)).all()
    if not terrains:
        raise HTTPException(status_code=404, detail="No terrains found for this user")

    return {
        "message": "Terrains retrieved successfully",
        "statuscode": 200,
        "terrains": [
            {
                "name": terrain.name, 
                "description": terrain.description, 
                "uuid": terrain.uuid,
                "heightmapResolution": terrain.heightmapResolution,
                "widthmapResolution": terrain.widthmapResolution,
                "size_X": terrain.size_X,
                "size_Y": terrain.size_Y,
                "size_Z": terrain.size_Z,
            } 
            for terrain in terrains]
    }


@app.get("/download-terrain/{terrain_uuid}")
async def download_terrain(terrain_uuid: str, db: sqlalchemy.orm.Session = Depends(get_db)):
    from models import Terrains as TerrainModel
    from models import FileStorage as FileStorageModel

    terrain = db.query(TerrainModel).filter(TerrainModel.uuid == terrain_uuid).first()
    if not terrain:
        raise HTTPException(status_code=404, detail="Terrain not found")

    raw_file = db.query(FileStorageModel).filter(
        FileStorageModel.terrain_uuid == terrain.uuid,
        FileStorageModel.filetype == "Heightmap"
    ).first()

    texture_files = db.query(FileStorageModel).filter(
        FileStorageModel.terrain_uuid == terrain.uuid,
        FileStorageModel.filetype == "Texture"
    ).all()

    if not raw_file:
        raise HTTPException(status_code=404, detail="Raw file not found")

    return {
        "message": "Terrain files retrieved successfully",
        "statuscode": 200,
        "terrain": {
            "name": terrain.name,
            "uuid": terrain.uuid,
            "rawFileName": raw_file.filename,
            "rawFileBytes": raw_file.file_data,
            "textureFiles": [{"textureFileName": tf.filename, "textureFileBytes": tf.file_data} for tf in texture_files]
        }
    }

@app.post("/create-level/{terrain_uuid}")
async def create_level(terrain_uuid: str, level: TerrainLevel, db: sqlalchemy.orm.Session = Depends(get_db)):
    from models import TerrainLevels as TerrainLevelModel



    new_level = TerrainLevelModel(
        name=level.name,
        description=level.description,
        terrain_uuid=terrain_uuid,
        start_X=level.start_X,
        start_Y=level.start_Y,
        start_Z=level.start_Z,
        end_X=level.end_X,
        end_Y=level.end_Y,
        end_Z=level.end_Z
    )
    user = db.query(Users).filter(Users.name == level.creator).first()
    if not user:
        raise HTTPException(status_code=400, detail="User not found")
    new_level.creator = user.uuid

    try:
        db.add(new_level)
        db.commit()
        return {
            "message": "Level created successfully",
            "statuscode": 200,
            "level": {
                "name": new_level.name,
                "uuid": new_level.uuid
            }
        }
    except Exception as e:
        db.rollback()
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        db.close()

@app.get("/levels/{terrain_uuid}")
async def get_levels(terrain_uuid: str, db: sqlalchemy.orm.Session = Depends(get_db)):
    from models import TerrainLevels as TerrainLevelModel

    levels = db.query(TerrainLevelModel).filter(TerrainLevelModel.terrain_uuid == terrain_uuid).all()
    if not levels:
        raise HTTPException(status_code=404, detail="No levels found for this terrain")
    
    creators = db.query(Users).filter(Users.uuid.in_([level.creator for level in levels])).all()
    creator_dict = {creator.uuid: creator.name for creator in creators} 

    return {
        "message": "Levels retrieved successfully",
        "statuscode": 200,
        "levels": [
            {
                "uuid": level.uuid,
                "name": level.name,
                "description": level.description,
                "start_X": level.start_X,
                "start_Y": level.start_Y,
                "start_Z": level.start_Z,
                "end_X": level.end_X,
                "end_Y": level.end_Y,
                "end_Z": level.end_Z,
                "creator": creator_dict.get(level.creator, "Unknown"),
                "creator_uuid": level.creator,
                "created_at": level.created_at.strftime("%Y-%m-%d %H:%M:%S") if level.created_at else None
            } 
            for level in levels]
    }

@app.post("/submit-level-score")
async def submit_level_score(score: LevelScore, db: sqlalchemy.orm.Session = Depends(get_db)):
    from models import LevelScores as LevelScoreModel

    user = db.query(Users).filter(Users.name == score.user).first()
    if not user:
        raise HTTPException(status_code=400, detail="User not found")

    new_score = LevelScoreModel(
        level_uuid=score.level_uuid,
        user_uuid=user.uuid,
        total2D_distance=score.total2D_distance,
        total3D_distance=score.total3D_distance,
        total_slope=score.total_slope,
        total_positive_slope=score.total_positive_slope,
        total_negative_slope=score.total_negative_slope,
        metabolic_cost=score.metabolic_cost
    )
    try:
        db.add(new_score)
        db.commit()
        return {
            "message": "Level score submitted successfully",
            "statuscode": 200
        }
    except Exception as e:
        db.rollback()
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        db.close()

@app.get("/level-scores/{level_uuid}")
async def get_level_scores(level_uuid: str, db: sqlalchemy.orm.Session = Depends(get_db)):
    from models import LevelScores as LevelScoreModel

    scores = db.query(LevelScoreModel).filter(LevelScoreModel.level_uuid == level_uuid).all()
    if not scores:
        raise HTTPException(status_code=404, detail="No scores found for this level")

    users = db.query(Users).filter(Users.uuid.in_([score.user_uuid for score in scores])).all()
    user_dict = {user.uuid: user.name for user in users} 

    return {
        "message": "Level scores retrieved successfully",
        "statuscode": 200,
        "scores": [
            {
                "uuid": score.uuid,
                "user": user_dict.get(score.user_uuid, "Unknown"),
                "total2D_distance": score.total2D_distance,
                "total3D_distance": score.total3D_distance,
                "total_slope": score.total_slope,
                "total_positive_slope": score.total_positive_slope,
                "total_negative_slope": score.total_negative_slope,
                "metabolic_cost": score.metabolic_cost,
                "created_at": score.created_at.strftime("%Y-%m-%d %H:%M:%S") if score.created_at else None
            } 
            for score in scores]
    }

if __name__ == '__main__':
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
