from PIL import Image
import numpy as np

# Cargar la imagen original
img = Image.open(r"C:\Users\Luca Acosta Iglesias\Documents\TFG\PathDrawing\TerFreser_919-2910\dem_10m.png")

# Obtener el tamaño original de la imagen
width, height = img.size

# Determinar el tamaño del recorte (aquí estamos recortando a 1024x1024)
new_size = 1024

# Calcular la zona a recortar (centrada)
left = (width - new_size) // 2
top = (height - new_size) // 2
right = left + new_size
bottom = top + new_size

# Recortar la imagen
img_cropped = img.crop((left, top, right, bottom))

# Convertir a numpy array para verificar valores si es necesario
img_array = np.array(img_cropped)

# Guardar la imagen recortada
img_cropped.save(r"C:\Users\Luca Acosta Iglesias\Documents\TFG\PathDrawing\TerFreser_919-2910\dem_FINAL.png")
