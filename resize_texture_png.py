from PIL import Image

def crop_and_save_png(input_path, output_path, crop_left, crop_top, crop_width, crop_height):
    # Carregar la imatge original
    image = Image.open(input_path)

    # Retallar la imatge amb les coordenades proporcionades
    cropped_image = image.crop((crop_left, crop_top, crop_left + crop_width, crop_top + crop_height))

    # Guardar la imatge retallada com a PNG
    cropped_image.save(output_path, format="PNG")

    print(f"Textura retallada i guardada en {output_path}")

# Paràmetres d'entrada
input_path = r"PathDrawing\TerFreser_919-2910\allaus.png"  # Ruta de la textura original
output_path = r"PathDrawing\TerFreser_919-2910\allaus_cropped.png"  # Ruta per guardar la nova textura

# Definir les dimensions del tall
crop_left = 0    # Coordenada X d'inici del tall
crop_top = 0     # Coordenada Y d'inici del tall
crop_width = 1025  # Amplada del tall (ajustar segons necessitat)
crop_height = 1025  # Altura del tall (ajustar segons necessitat)

# Retallar i guardar la textura com a PNG
crop_and_save_png(input_path, output_path, crop_left, crop_top, crop_width, crop_height)
