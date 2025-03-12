from PIL import Image

def crop_and_save_png(input_path, output_path, crop_left, crop_top, crop_width, crop_height):
    # Cargar la imagen en su formato original
    image = Image.open(input_path)

    # Recortar la imagen con las coordenadas proporcionadas
    cropped_image = image.crop((crop_left, crop_top, crop_left + crop_width, crop_top + crop_height))

    # Guardar la imagen recortada como PNG
    cropped_image.save(output_path, format="PNG")

    print(f"Textura recortada y guardada en {output_path}")

# Parámetros de entrada
input_path = r"PathDrawing\TerFreser_919-2910\curv-tangential_w11.png"  # Ruta de la textura original
output_path = r"PathDrawing\TerFreser_919-2910\curv-tangential_w11_cropped.png"  # Ruta para guardar la nueva textura

# Definir las dimensiones del recorte
crop_left = 0    # Coordenada X de inicio del recorte
crop_top = 0     # Coordenada Y de inicio del recorte
crop_width = 1024  # Ancho del recorte (ajustar según necesidad)
crop_height = 1024  # Alto del recorte (ajustar según necesidad)

# Recortar y guardar la textura como PNG
crop_and_save_png(input_path, output_path, crop_left, crop_top, crop_width, crop_height)
