#!/bin/bash

# Norta API Seed Data Script
# Creates sample users, posts, and interactions for testing

API_URL="${API_URL:-http://localhost:5000/api}"

echo "Seeding Norta database..."
echo "API URL: $API_URL"
echo ""

# Function to register and get token
register_user() {
    local email=$1
    local password=$2
    local displayName=$3

    echo "Creating user: $displayName ($email)"

    response=$(curl -s -X POST "$API_URL/auth/register" \
        -H "Content-Type: application/json" \
        -d "{\"email\":\"$email\",\"password\":\"$password\",\"displayName\":\"$displayName\"}")

    token=$(echo $response | jq -r '.accessToken')
    userId=$(echo $response | jq -r '.userId')

    echo "  Token: ${token:0:20}..."
    echo "  UserID: $userId"
    echo ""

    echo "$token|$userId"
}

# Create users
echo "=== Creating Users ==="
ALICE=$(register_user "alice@norta.app" "Password123!" "Alice Johnson")
ALICE_TOKEN=$(echo $ALICE | cut -d'|' -f1)
ALICE_ID=$(echo $ALICE | cut -d'|' -f2)

BOB=$(register_user "bob@norta.app" "Password123!" "Bob Smith")
BOB_TOKEN=$(echo $BOB | cut -d'|' -f1)
BOB_ID=$(echo $BOB | cut -d'|' -f2)

CAROL=$(register_user "carol@norta.app" "Password123!" "Carol Williams")
CAROL_TOKEN=$(echo $CAROL | cut -d'|' -f1)
CAROL_ID=$(echo $CAROL | cut -d'|' -f2)

echo "=== Creating Follows ==="
echo "Alice follows Bob and Carol..."
curl -s -X POST "$API_URL/users/$BOB_ID/follow" \
    -H "Authorization: Bearer $ALICE_TOKEN" > /dev/null
curl -s -X POST "$API_URL/users/$CAROL_ID/follow" \
    -H "Authorization: Bearer $ALICE_TOKEN" > /dev/null

echo "Bob follows Alice..."
curl -s -X POST "$API_URL/users/$ALICE_ID/follow" \
    -H "Authorization: Bearer $BOB_TOKEN" > /dev/null

echo "Carol follows Alice and Bob..."
curl -s -X POST "$API_URL/users/$ALICE_ID/follow" \
    -H "Authorization: Bearer $CAROL_TOKEN" > /dev/null
curl -s -X POST "$API_URL/users/$BOB_ID/follow" \
    -H "Authorization: Bearer $CAROL_TOKEN" > /dev/null

echo ""
echo "=== Creating Posts ==="

# Alice's posts
echo "Creating Alice's posts..."
ALICE_POST1=$(curl -s -X POST "$API_URL/posts" \
    -H "Authorization: Bearer $ALICE_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"caption":"Just joined Norta! 🎉","imageUrl":null}' | jq -r '.id')

ALICE_POST2=$(curl -s -X POST "$API_URL/posts" \
    -H "Authorization: Bearer $ALICE_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"caption":"Beautiful sunset today 🌅","imageUrl":null}' | jq -r '.id')

# Bob's posts
echo "Creating Bob's posts..."
BOB_POST1=$(curl -s -X POST "$API_URL/posts" \
    -H "Authorization: Bearer $BOB_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"caption":"Coffee time ☕","imageUrl":null}' | jq -r '.id')

# Carol's posts
echo "Creating Carol's posts..."
CAROL_POST1=$(curl -s -X POST "$API_URL/posts" \
    -H "Authorization: Bearer $CAROL_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"caption":"Loving this new social app!","imageUrl":null}' | jq -r '.id')

echo ""
echo "=== Creating Likes ==="

# Alice likes Bob and Carol's posts
curl -s -X POST "$API_URL/posts/$BOB_POST1/like" \
    -H "Authorization: Bearer $ALICE_TOKEN" > /dev/null
curl -s -X POST "$API_URL/posts/$CAROL_POST1/like" \
    -H "Authorization: Bearer $ALICE_TOKEN" > /dev/null

# Bob likes Alice's posts
curl -s -X POST "$API_URL/posts/$ALICE_POST1/like" \
    -H "Authorization: Bearer $BOB_TOKEN" > /dev/null
curl -s -X POST "$API_URL/posts/$ALICE_POST2/like" \
    -H "Authorization: Bearer $BOB_TOKEN" > /dev/null

# Carol likes everyone's posts
curl -s -X POST "$API_URL/posts/$ALICE_POST1/like" \
    -H "Authorization: Bearer $CAROL_TOKEN" > /dev/null
curl -s -X POST "$API_URL/posts/$BOB_POST1/like" \
    -H "Authorization: Bearer $CAROL_TOKEN" > /dev/null

echo ""
echo "=== Creating Comments ==="

curl -s -X POST "$API_URL/posts/$ALICE_POST1/comments" \
    -H "Authorization: Bearer $BOB_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"text":"Welcome to Norta! 👋"}' > /dev/null

curl -s -X POST "$API_URL/posts/$ALICE_POST2/comments" \
    -H "Authorization: Bearer $CAROL_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"text":"Amazing photo!"}' > /dev/null

curl -s -X POST "$API_URL/posts/$BOB_POST1/comments" \
    -H "Authorization: Bearer $ALICE_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"text":"I need coffee too!"}' > /dev/null

echo ""
echo "✅ Seed data created successfully!"
echo ""
echo "Test accounts:"
echo "  - alice@norta.app / Password123!"
echo "  - bob@norta.app / Password123!"
echo "  - carol@norta.app / Password123!"
echo ""
echo "You can now log in to the mobile app with any of these accounts."
