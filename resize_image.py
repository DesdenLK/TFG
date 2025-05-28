import numpy as np
from PIL import Image

def crop_and_save_raw(input_path, output_path, crop_left, crop_top, crop_width, crop_height):
    # Cargar la imagen en 16 bits
    image = Image.open(input_path).convert("I;16")  # 'I;16' es el modo de 16 bits

    # Recortar la imagen con las coordenadas proporcionadas
    cropped_image = image.crop((crop_left, crop_top, crop_left + crop_width, crop_top + crop_height))

    # Convertir a un array de numpy (16 bits) para asegurarnos de que está en el formato correcto
    cropped_array = np.array(cropped_image, dtype=np.uint16)

    # Guardar el heightmap como archivo RAW de 16 bits
    cropped_array.tofile(output_path)  # Guarda los datos binarios sin encabezado

    print(f"Heightmap recortado y guardado en {output_path} como archivo RAW de 16 bits")

# Parámetros de entrada
input_path = r".\TFG\PathDrawing\Tavascan_934-2864\dem_10m.png"  # Ruta de la imagen original
output_path = r".\TFG\PathDrawing\Tavascan_934-2864\dem_FINAL.raw"  # Ruta para guardar el RAW

# Definir las dimensiones del recorte
crop_left = 0    # Coordenada X de inicio del recorte
crop_top = 0     # Coordenada Y de inicio del recorte
crop_width = 1024  # Ancho del recorte (ajustar según necesidad)
crop_height = 1024  # Alto del recorte (ajustar según necesidad)

# Recortar y guardar como RAW
crop_and_save_raw(input_path, output_path, crop_left, crop_top, crop_width, crop_height)
