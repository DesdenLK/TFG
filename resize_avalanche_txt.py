def crop_avalanche_txt(input_path, output_path, crop_left, crop_top, crop_width, crop_height, total_width, total_height):
    # Leer todos los valores del txt
    with open(input_path, 'r') as f:
        content = f.read()

    # Parsear todos los valores (números enteros)
    tokens = content.split()
    if len(tokens) != total_width * total_height:
        raise ValueError(f"El archivo no tiene el tamaño esperado: {len(tokens)} valores, esperaba {total_width * total_height}")

    # Convertir a matriz 2D para facilitar el recorte
    avalanche_matrix = []
    idx = 0
    for _ in range(total_height):
        row = [int(tokens[idx + i]) for i in range(total_width)]
        avalanche_matrix.append(row)
        idx += total_width

    # Recortar la matriz
    cropped_matrix = []
    for y in range(crop_top, crop_top + crop_height):
        cropped_row = avalanche_matrix[y][crop_left : crop_left + crop_width]
        cropped_matrix.append(cropped_row)

    # Guardar la matriz recortada en un nuevo txt
    with open(output_path, 'w') as f_out:
        for row in cropped_matrix:
            f_out.write(' '.join(map(str, row)) + '\n')

    print(f"Avalanche recortado guardado en {output_path}")


input_path = r".\PathDrawing\TerFreser_919-2910\allaus.txt"  # Ruta de la imagen original
output_path = r".\PathDrawing\TerFreser_919-2910\allaus_cropped.txt"  # Ruta para guardar el RAW

# Definir las dimensiones del recorte
crop_left = 0    # Coordenada X de inicio del recorte
crop_top = 0     # Coordenada Y de inicio del recorte
crop_width = 1025  # Ancho del recorte (ajustar según necesidad)
crop_height = 1025  # Alto del recorte (ajustar según necesidad)
total_width = 1740  # Ancho total del archivo original
total_height = 1392  # Alto total del archivo original

# Recortar y guardar como RAW
crop_avalanche_txt(input_path, output_path, crop_left, crop_top, crop_width, crop_height, total_width, total_height)