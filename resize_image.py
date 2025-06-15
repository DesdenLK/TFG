import numpy as np
from PIL import Image

def crop_and_save_raw(input_path, output_path, crop_left, crop_top, crop_width, crop_height):
    # Carregar la imatge original
    image = Image.open(input_path).convert("I;16")  # 'I;16' es el modo de 16 bits

    # Retallar la imatge amb les coordenades proporcionades
    cropped_image = image.crop((crop_left, crop_top, crop_left + crop_width, crop_top + crop_height))

    # Convertir a un array de numpy (16 bits) per assegurar-nos que està en el format correcte
    cropped_array = np.array(cropped_image, dtype=np.uint16)

    # Guardar el heightmap com a arxiu RAW de 16 bits
    cropped_array.tofile(output_path)  # Guarda els dades binàries sense capçalera

    print(f"Heightmap retallat i guardat en {output_path} com a arxiu RAW de 16 bits")

# Paràmetres d'entrada
input_path = r".\TFG\PathDrawing\Tavascan_934-2864\dem_10m.png"  # Ruta de la imatge original
output_path = r".\TFG\PathDrawing\Tavascan_934-2864\dem_FINAL.raw"  # Ruta per guardar el RAW

# Definir les dimensions del tall
crop_left = 0    # Coordenada X d'inici del tall
crop_top = 0     # Coordenada Y d'inici del tall
crop_width = 1024  # Amplada del tall (ajustar segons necessitat)
crop_height = 1024  # Altura del tall (ajustar segons necessitat)

# Retallar i guardar el nou arxiu d'allaus
crop_and_save_raw(input_path, output_path, crop_left, crop_top, crop_width, crop_height)
