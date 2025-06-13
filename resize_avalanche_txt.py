def crop_avalanche_txt(input_path, output_path, crop_left, crop_top, crop_width, crop_height, total_width, total_height):
    # Llegir l'arxiu de text
    with open(input_path, 'r') as f:
        content = f.read()

    # Obtenir els valors de l'arxiu
    tokens = content.split()
    if len(tokens) != total_width * total_height:
        raise ValueError(f"El fitxer no té la mida esperada: {len(tokens)} valors, esperava {total_width * total_height}")

    # Crear la matriu d'allaus
    avalanche_matrix = []
    idx = 0
    for _ in range(total_height):
        row = [int(tokens[idx + i]) for i in range(total_width)]
        avalanche_matrix.append(row)
        idx += total_width

    # Retalla la matriu
    cropped_matrix = []
    for y in range(crop_top, crop_top + crop_height):
        cropped_row = avalanche_matrix[y][crop_left : crop_left + crop_width]
        cropped_matrix.append(cropped_row)

    # Guarda la nova matriu retallada en un nou arxiu
    with open(output_path, 'w') as f_out:
        for row in cropped_matrix:
            f_out.write(' '.join(map(str, row)) + '\n')

    print(f"Avalanche retallat guardat en {output_path}")


input_path = r".\PathDrawing\TerFreser_919-2910\allaus.txt"  # Ruta de l'arxiu original
output_path = r".\PathDrawing\TerFreser_919-2910\allaus_cropped.txt"  # Ruta del nou arxiu retallat

# Definir les dimensions 
crop_left = 0    # Coordenada X d'inici del tall
crop_top = 0     # Coordenada Y d'inici del tall
crop_width = 1025  # Amplada del tall (ajustar segons necessitat)
crop_height = 1025  # Altura del tall (ajustar segons necessitat)
total_width = 1740  # Amplada total de l'arxiu original
total_height = 1392  # Altura total de l'arxiu original

# Retallar i guardar el nou arxiu d'allaus
crop_avalanche_txt(input_path, output_path, crop_left, crop_top, crop_width, crop_height, total_width, total_height)