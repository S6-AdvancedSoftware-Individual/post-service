FROM postgres:15

# Environment variables for database configuration
ENV POSTGRES_DB=postdb
ENV POSTGRES_USER=admin
ENV POSTGRES_PASSWORD=admin123

# Copy any initialization scripts (if needed)
# COPY ./init.sql /docker-entrypoint-initdb.d/

# Expose the PostgreSQL port
EXPOSE 5432

# The official postgres image already includes CMD ["postgres"] 