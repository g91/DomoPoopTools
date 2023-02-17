#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <sys/socket.h>
#include <sys/types.h>
#include <netinet/in.h>
#include <pthread.h>
#include <unistd.h>

#define BUF_SIZE 1024

#define PACKET_TYPE_HELLO 1
#define PACKET_TYPE_MESSAGE 2
#define PACKET_TYPE_COMMAND 3

struct packet_header {
    int type;
    int size;
    int id;
};

struct user {
    int id;
    char username[BUF_SIZE];
    char password[BUF_SIZE];
    int is_logged_in;
    int in_chat_room;
    int socket_id;
};

int num_users = 0;
struct user *users = NULL;

int num_chat_room_users = 0;
int *chat_room_users = NULL;

int create_server_socket(int port) {
    int server_socket_id = socket(AF_INET, SOCK_STREAM, 0);
    if (server_socket_id == -1) {
        perror("socket creation failed");
        exit(EXIT_FAILURE);
    }

    struct sockaddr_in server_address;
    memset(&server_address, 0, sizeof(server_address));
    server_address.sin_family = AF_INET;
    server_address.sin_addr.s_addr = INADDR_ANY;
    server_address.sin_port = htons(port);

    if (bind(server_socket_id, (struct sockaddr *) &server_address, sizeof(server_address)) == -1) {
        perror("bind failed");
        exit(EXIT_FAILURE);
    }

    if (listen(server_socket_id, 5) == -1) {
        perror("listen failed");
        exit(EXIT_FAILURE);
    }

    return server_socket_id;
}

int get_user_by_id(int id) {
    for (int i = 0; i < num_users; i++) {
        if (users[i].id == id) {
            return i;
        }
    }
    return -1;
}

int get_user_by_username(const char *username) {
    for (int i = 0; i < num_users; i++) {
        if (strcmp(users[i].username, username) == 0) {
            return i;
        }
    }
    return -1;
}

void handle_hello_packet(struct packet_header *header, char *payload, int socket_id) {
    struct user *user = NULL;
    int user_index = -1;

    int username_len = strcspn(payload, " ");
    if (username_len >= BUF_SIZE) {
        fprintf(stderr, "received username that is too long\n");
        return;
    }

    char *password = payload + username_len + 1;
    int password_len = strcspn(password, "\r\n");

    if (username_len == 0 || password_len == 0) {
        fprintf(stderr, "received invalid username or password\n");
        return;
    }

    password[password_len] = 0;

    user_index = get_user_by_username(payload);
    if (user_index != -1) {
        user = &users[user_index];
        if (strcmp(user->password, password) != 0) {
            user_index = -1;
            user = NULL;
        }
    }

    if (user == NULL) {
        user_index = num_users++;
        users = realloc(users, num_users * sizeof(struct user));
        user = &users[user_index];
        user->id = user_index;
        strncpy(user->username, payload, username_len);
        user->username[username_len] = 0;
        strncpy(user->password, password, password_len);
        user->password[password_len] = 0;
        user->is_logged_in = 0;
		        user->in_chat_room = 0;
        user->socket_id = socket_id;
    } else {
        user->socket_id = socket_id;
        if (user->is_logged_in) {
            struct packet_header login_header;
            login_header.type = PACKET_TYPE_COMMAND;
            login_header.size = 0;
            login_header.id = -1;
            char login_payload[BUF_SIZE];
            snprintf(login_payload, BUF_SIZE, "You are already logged in as %s.\n", user->username);
            if (send(socket_id, &login_header, sizeof(struct packet_header), 0) == -1) {
                perror("send failed");
                return;
            }
            if (send(socket_id, login_payload, strlen(login_payload), 0) == -1) {
                perror("send failed");
                return;
            }
            return;
        }
    }

    user->is_logged_in = 1;

    struct packet_header response_header;
    response_header.type = PACKET_TYPE_COMMAND;
    response_header.size = 0;
    response_header.id = -1;
    char response_payload[BUF_SIZE];
    snprintf(response_payload, BUF_SIZE, "Welcome, %s!\n", user->username);
    if (send(socket_id, &response_header, sizeof(struct packet_header), 0) == -1) {
        perror("send failed");
        return;
    }
    if (send(socket_id, response_payload, strlen(response_payload), 0) == -1) {
        perror("send failed");
        return;
    }
}

void handle_message_packet(struct packet_header *header, char *payload, int socket_id) {
    struct user *user = NULL;
    for (int i = 0; i < num_users; i++) {
        if (users[i].socket_id == socket_id) {
            user = &users[i];
            break;
        }
    }
    if (!user || !user->is_logged_in) {
        struct packet_header error_header;
        error_header.type = PACKET_TYPE_COMMAND;
        error_header.size = 0;
        error_header.id = header->id;
        char error_payload[BUF_SIZE];
        snprintf(error_payload, BUF_SIZE, "You are not logged in.\n");
        if (send(socket_id, &error_header, sizeof(struct packet_header), 0) == -1) {
            perror("send failed");
            return;
        }
        if (send(socket_id, error_payload, strlen(error_payload), 0) == -1) {
            perror("send failed");
            return;
        }
        return;
    }
    if (!user->in_chat_room) {
        struct packet_header error_header;
        error_header.type = PACKET_TYPE_COMMAND;
        error_header.size = 0;
        error_header.id = header->id;
        char error_payload[BUF_SIZE];
        snprintf(error_payload, BUF_SIZE, "You are not in a chat room.\n");
        if (send(socket_id, &error_header, sizeof(struct packet_header), 0) == -1) {
            perror("send failed");
            return;
        }
        if (send(socket_id, error_payload, strlen(error_payload), 0) == -1) {
            perror("send failed");
            return;
        }
        return;
    }
    for (int i = 0; i < num_chat_room_users; i++) {
        if (chat_room_users[i] != user->id && users[chat_room_users[i]].is_logged_in) {
            if (send(users[chat_room_users[i]].socket_id, header, sizeof(struct packet_header), 0) == -1)
            if (send(users[chat_room_users[i]].socket_id, payload, header->size, 0) == -1) {
                perror("send failed");
                return;
            }
        }
    }
}

void handle_command_packet(struct packet_header *header, char *payload, int socket_id) {
    struct user *user = NULL;
    for (int i = 0; i < num_users; i++) {
        if (users[i].socket_id == socket_id) {
            user = &users[i];
            break;
        }
    }
    if (!user || !user->is_logged_in) {
        struct packet_header error_header;
        error_header.type = PACKET_TYPE_COMMAND;
        error_header.size = 0;
        error_header.id = header->id;
        char error_payload[BUF_SIZE];
        snprintf(error_payload, BUF_SIZE, "You are not logged in.\n");
        if (send(socket_id, &error_header, sizeof(struct packet_header), 0) == -1) {
            perror("send failed");
            return;
        }
        if (send(socket_id, error_payload, strlen(error_payload), 0) == -1) {
            perror("send failed");
            return;
        }
        return;
    }

    if (strncmp(payload, "/join", 5) == 0) {
        if (user->in_chat_room) {
            struct packet_header error_header;
            error_header.type = PACKET_TYPE_COMMAND;
            error_header.size = 0;
            error_header.id = header->id;
            char error_payload[BUF_SIZE];
            snprintf(error_payload, BUF_SIZE, "You are already in the chat room.\n");
            if (send(socket_id, &error_header, sizeof(struct packet_header), 0) == -1) {
                perror("send failed");
                return;
            }
            if (send(socket_id, error_payload, strlen(error_payload), 0) == -1) {
                perror("send failed");
                return;
            }
            return;
        }
        user->in_chat_room = 1;
        chat_room_users = realloc(chat_room_users, ++num_chat_room_users * sizeof(int));
        chat_room_users[num_chat_room_users - 1] = user->id;
        struct packet_header response_header;
        response_header.type = PACKET_TYPE_COMMAND;
        response_header.size = 0;
        response_header.id = header->id;
        char response_payload[BUF_SIZE];
        snprintf(response_payload, BUF_SIZE, "You have joined the chat room.\n");
        if (send(socket_id, &response_header, sizeof(struct packet_header), 0) == -1) {
            perror("send failed");
            return;
        }
        if (send(socket_id, response_payload, strlen(response_payload), 0) == -1) {
            perror("send failed");
            return;
        }
    } else if (strncmp(payload, "/leave", 6) == 0) {
        if (!user->in_chat_room) {
            struct packet_header error_header;
            error_header.type = PACKET_TYPE_COMMAND;
            error_header.size = 0;
            error_header.id = header->id;
            char error_payload[BUF_SIZE];
            snprintf(error_payload, BUF_SIZE, "You are not in the chat room.\n");
            if (send(socket_id, &error_header, sizeof(struct packet_header), 0) == -1) {
                perror("send failed");
                return;
            }
            if (send(socket_id, error_payload, strlen(error_payload), 0) == -1) {
                perror("send failed");
                return;
			}
            return;
        }
        user->in_chat_room = 0;
        for (int i = 0; i < num_chat_room_users; i++) {
            if (chat_room_users[i] == user->id) {
                chat_room_users[i] = chat_room_users[--num_chat_room_users];
                chat_room_users = realloc(chat_room_users, num_chat_room_users * sizeof(int));
                break;
            }
        }
        struct packet_header response_header;
        response_header.type = PACKET_TYPE_COMMAND;
        response_header.size = 0;
        response_header.id = header->id;
        char response_payload[BUF_SIZE];
        snprintf(response_payload, BUF_SIZE, "You have left the chat room.\n");
        if (send(socket_id, &response_header, sizeof(struct packet_header), 0) == -1) {
            perror("send failed");
            return;
        }
        if (send(socket_id, response_payload, strlen(response_payload), 0) == -1) {
            perror("send failed");
            return;
        }
    } else {
        struct packet_header error_header;
        error_header.type = PACKET_TYPE_COMMAND;
        error_header.size = 0;
        error_header.id = header->id;
        char error_payload[BUF_SIZE];
        snprintf(error_payload, BUF_SIZE, "Invalid command.\n");
        if (send(socket_id, &error_header, sizeof(struct packet_header), 0) == -1) {
            perror("send failed");
            return;
        }
        if (send(socket_id, error_payload, strlen(error_payload), 0) == -1) {
            perror("send failed");
            return;
        }
    }
}

void handle_packet(struct packet_header *header, char *payload, int socket_id) {
    switch (header->type) {
        case PACKET_TYPE_HELLO:
            handle_hello_packet(header, payload, socket_id);
            break;
        case PACKET_TYPE_MESSAGE:
            handle_message_packet(header, payload, socket_id);
            break;
        case PACKET_TYPE_COMMAND:
            handle_command_packet(header, payload, socket_id);
            break;
        default:
            fprintf(stderr, "received packet with unknown type\n");
            break;
    }
}

void *client_thread(void *arg) {
    int client_socket_id = *(int *) arg;

    struct packet_header header;
    while (recv(client_socket_id, &header, sizeof(struct packet_header), 0) != -1) {
        char payload[BUF_SIZE];
        if (recv(client_socket_id, payload, header.size, 0) == -1) {
            perror("recv failed");
            break;
        }
        handle_packet(&header, payload, client_socket_id);
    }

    for (int i = 0; i < num_users; i++) {
        if (users[i].socket_id == client_socket_id) {
            users[i].socket_id = -1;
            users[i].is_logged_in = 0;
            break;
        }
    }

    close(client_socket_id);
    return NULL;
}

int main(int argc, char *argv[]) {
    if (argc != 2) {
        fprintf(stderr, "usage: %s <port>\n", argv[0]);
        return 1;
    }

    int port = atoi(argv[1]);

    int server_socket_id = create_server_socket(port);

    while (1) {
        int client_socket_id = accept(server_socket_id, NULL, NULL);
        if (client_socket_id == -1) {
            perror("accept failed");
            continue;
        }
        pthread_t tid;
        pthread_create(&tid, NULL, client_thread, &client_socket_id);
    }

    return 0;
}


