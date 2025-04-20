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

if __name__ == '__main__':
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
