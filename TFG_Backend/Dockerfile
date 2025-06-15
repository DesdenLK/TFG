FROM python:3.11-slim

ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1
ENV PORT=8080

WORKDIR /app

RUN apt-get update && apt-get install -y gcc

COPY requirements.txt .

RUN pip install --upgrade pip && pip install -r requirements.txt

COPY app.py .
COPY models.py .
COPY migrations ./migrations
COPY alembic.ini .

EXPOSE 8080

# CMD con shell para correr los dos comandos
CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "8080"]
