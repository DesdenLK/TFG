from typing import List
from fastapi import FastAPI, HTTPException, Request, Depends
from sqlalchemy import create_engine
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
            print(texture.textureFileName)
            print(texture.textureFileBytes)
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
if __name__ == '__main__':
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
